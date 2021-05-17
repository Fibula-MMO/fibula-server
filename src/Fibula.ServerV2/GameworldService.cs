// -----------------------------------------------------------------
// <copyright file="GameworldService.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Definitions.Constants;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Scheduling.Contracts.Delegates;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Delegates;
    using Fibula.ServerV2.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Extensions;
    using Fibula.ServerV2.Creatures;
    using Fibula.ServerV2.Notifications;
    using Fibula.ServerV2.Operations;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents the game world service.
    /// </summary>
    public class GameworldService : IGameworldService
    {
        /// <summary>
        /// Stores a reference to the application wide context.
        /// </summary>
        private readonly IApplicationContext applicationContext;

        /// <summary>
        /// A reference to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Stores the scheduler used by the game.
        /// </summary>
        private readonly IScheduler scheduler;

        /// <summary>
        /// The current map instance.
        /// </summary>
        private readonly IMap map;

        /// <summary>
        /// The creature manager instance.
        /// </summary>
        private readonly ICreatureManager creatureManager;

        /// <summary>
        /// Gets the item factory instance.
        /// </summary>
        private readonly IItemFactory itemFactory;

        /// <summary>
        /// Gets the creature factory instance.
        /// </summary>
        private readonly ICreatureFactory creatureFactory;

        /// <summary>
        /// The pathfinder algorithm in use.
        /// </summary>
        private readonly IPathFinder pathFinder;

        /// <summary>
        /// Gets the monster spawns in the game.
        /// </summary>
        private readonly IEnumerable<Spawn> monsterSpawns;

        /// <summary>
        /// The predefined items used in the game.
        /// </summary>
        private readonly IPredefinedItemSet predefinedItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameworldService"/> class.
        /// </summary>
        /// <param name="applicationContext">A reference to the application context.</param>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="map">A reference to the map.</param>
        /// <param name="creatureManager">A reference to the creature manager in use.</param>
        /// <param name="itemFactory">A reference to the item factory in use.</param>
        /// <param name="creatureFactory">A reference to the creature factory in use.</param>
        /// <param name="pathFinderAlgo">A reference to the path finding algorithm in use.</param>
        /// <param name="predefinedItemSet">A reference to the predefined item set declared.</param>
        /// <param name="monsterSpawnsLoader">A reference to the monster spawns loader.</param>
        /// <param name="scheduler">A reference to the global scheduler instance.</param>
        public GameworldService(
            IApplicationContext applicationContext,
            ILogger<GameworldService> logger,
            IMap map,
            ICreatureManager creatureManager,
            IItemFactory itemFactory,
            ICreatureFactory creatureFactory,
            IPathFinder pathFinderAlgo,
            IPredefinedItemSet predefinedItemSet,
            IMonsterSpawnLoader monsterSpawnsLoader,
            IScheduler scheduler)
        {
            applicationContext.ThrowIfNull(nameof(applicationContext));
            logger.ThrowIfNull(nameof(logger));
            map.ThrowIfNull(nameof(map));
            creatureManager.ThrowIfNull(nameof(creatureManager));
            itemFactory.ThrowIfNull(nameof(itemFactory));
            creatureFactory.ThrowIfNull(nameof(creatureFactory));
            pathFinderAlgo.ThrowIfNull(nameof(pathFinderAlgo));
            predefinedItemSet.ThrowIfNull(nameof(predefinedItemSet));
            monsterSpawnsLoader.ThrowIfNull(nameof(monsterSpawnsLoader));
            scheduler.ThrowIfNull(nameof(scheduler));

            this.applicationContext = applicationContext;
            this.logger = logger;
            this.map = map;
            this.creatureManager = creatureManager;
            this.itemFactory = itemFactory;
            this.creatureFactory = creatureFactory;
            this.pathFinder = pathFinderAlgo;
            this.predefinedItems = predefinedItemSet;
            this.scheduler = scheduler;

            this.itemFactory.ItemCreated += this.AfterItemIsCreated;

            // Load the spawns
            this.monsterSpawns = monsterSpawnsLoader.LoadSpawns();

            // Hook some event handlers.
            this.scheduler.EventFired += this.HandleSchedulerFiredEvent;
            this.map.WindowLoaded += this.OnMapWindowLoaded;

            this.IpAddress = IPAddress.TryParse(applicationContext.Options.World.IpAddress, out IPAddress address) ? address.GetAddressBytes() : Array.Empty<byte>();
            this.Port = applicationContext.Options.World.Port.GetValueOrDefault();

            this.State = WorldState.Loading;
            this.LightColor = LightConstants.WhiteColor;
            this.LightLevel = LightConstants.WorldDay;
        }

        /// <summary>
        /// Event fired when there is a notification ready to be broadcasted to the <see cref="IGameworldService"/>'s subscribers.
        /// </summary>
        public event GameworldNotificationReadyHandler NotificationReady;

        /// <summary>
        /// Gets the bytes that represent the IPAddress at which this world is hosted.
        /// </summary>
        public byte[] IpAddress { get; }

        /// <summary>
        /// Gets the port number at which this world is hosted.
        /// </summary>
        public ushort Port { get; }

        /// <summary>
        /// Gets the state of the world.
        /// </summary>
        public WorldState State { get; private set; }

        /// <summary>
        /// Gets the current light color of the world.
        /// </summary>
        public byte LightColor { get; private set; }

        /// <summary>
        /// Gets the current light level of the world.
        /// </summary>
        public byte LightLevel { get; private set; }

        /// <summary>
        /// Runs the main game processing thread which begins advancing time on the game engine.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(
                async () =>
                {
                    // start the scheduler.
                    var schedulerTask = this.scheduler.RunAsync(cancellationToken);

                    var miscellaneusEventsTask = Task.Factory.StartNew(this.MiscellaneousEventsLoop, cancellationToken, TaskCreationOptions.LongRunning);

                    // Open the game world!
                    this.State = WorldState.Open;

                    await Task.WhenAll(miscellaneusEventsTask, schedulerTask);
                },
                cancellationToken);

            // return this to allow other IHostedService-s to start.
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogWarning($"Cancellation requested on game instance, beginning shut-down...");

            this.itemFactory.ItemCreated -= this.AfterItemIsCreated;

            // TODO: probably save game state here.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs a player into the game.
        /// </summary>
        /// <param name="characterMetadata">The metadata for the player's creation.</param>
        /// <returns>The id reserved for the player logging in.</returns>
        public uint LogPlayerIn(CharacterEntity characterMetadata)
        {
            characterMetadata.ThrowIfNull(nameof(characterMetadata));

            var newPlayerId = Player.ReserveNewId();

            var loginOp = new LogInOperation(requestorId: 0, newPlayerId, characterMetadata, this.LightLevel, this.LightColor);

            this.DispatchOperation(loginOp);

            return newPlayerId;
        }

        /// <summary>
        /// Logs a player out of the game.
        /// </summary>
        /// <param name="player">The player to log out.</param>
        public void LogPlayerOut(IPlayer player)
        {
            if (player == null)
            {
                return;
            }

            // var logOutOp = new LogOutOperation(player.Id, player);

            // this.DispatchOperation(logOutOp);
        }

        /// <summary>
        /// Attempts to add a creature to the game at a given location on the map.
        /// </summary>
        /// <param name="targetLocation">The location to place the creature at.</param>
        /// <param name="creature">The creature to place.</param>
        /// <returns>True if the creature is successfully placed, false otherwise.</returns>
        public bool AddCreatureToGame(Location targetLocation, ICreature creature)
        {
            creature.ThrowIfNull(nameof(creature));

            if (!this.map.HasTileAt(targetLocation, out ITile targetTile))
            {
                return false;
            }

            if (!targetTile.AddCreature(creature))
            {
                this.logger.LogDebug($"Unable to place {creature.Name} at {targetTile.Location}.");

                return false;
            }

            // The creature is now placed in the map (and in the game).
            this.creatureManager.RegisterCreature(creature);

            creature.LocationChanged += this.AfterThingLocationChanged;

            this.logger.LogDebug($"Placed {creature.Name} at {targetTile.Location}.");

            var placedAtIndex = targetTile.GetIndexOfThing(creature);

            if (placedAtIndex != byte.MaxValue)
            {
                var player = creature as IPlayer;

                if (player != null)
                {
                    var loginNotification = new PlayerLogInNotification(player, creature.Location, this.map.DescribeAt(player, player.Location), AnimatedEffect.BubbleBlue);

                    this.SendNotification(loginNotification);
                }

                var spectators = player != null ? this.map.FindPlayersThatCanSee(creature.Location).Except(player.YieldSingleItem()) : this.map.FindPlayersThatCanSee(creature.Location);

                if (spectators.Any())
                {
                    var creatureAddedNotification = new CreatureAddedNotification(spectators, creature, creature.Location, placedAtIndex, AnimatedEffect.BubbleBlue);

                    this.SendNotification(creatureAddedNotification);
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to remove a creature from the game.
        /// </summary>
        /// <param name="creature">The creature to remove.</param>
        /// <returns>True if the creature is successfully removed from the game, false otherwise.</returns>
        public bool RemoveCreatureFromGame(ICreature creature)
        {
            if (!this.map.HasTileAt(creature.Location, out ITile fromTile))
            {
                return false;
            }

            var oldStackpos = fromTile.GetIndexOfThing(creature);

            if (oldStackpos == byte.MaxValue || !fromTile.RemoveCreature(creature))
            {
                this.logger.LogDebug($"Unable to remove {creature.Name} from {fromTile.Location}.");

                return false;
            }

            creature.LocationChanged -= this.AfterThingLocationChanged;

            var spectators = (creature is IPlayer player) ? this.map.FindPlayersThatCanSee(creature.Location).Union(player.YieldSingleItem()) : this.map.FindPlayersThatCanSee(creature.Location);
            var notification = new CreatureRemovedNotification(spectators, creature, oldStackpos);

            this.SendNotification(notification);

            return true;
        }

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="notification">The notification to send.</param>
        public void SendNotification(INotification notification)
        {
            if (notification == null)
            {
                return;
            }

            this.NotificationReady?.Invoke(this, notification);
        }

        /// <summary>
        /// Handles the aftermath of a location change from a thing.
        /// </summary>
        /// <param name="thing">The thing which's location changed.</param>
        /// <param name="previousLocation">The previous location of the thing.</param>
        private void AfterThingLocationChanged(IThing thing, Location previousLocation)
        {
            thing.ThrowIfNull(nameof(thing));

            this.logger.LogTrace($"{thing} changed location from {previousLocation} to {thing.Location}.");
        }

        /// <summary>
        /// Handles miscellaneous stuff on the game world, such as world light.
        /// </summary>
        /// <param name="tokenState">The state object which gets casted into a <see cref="CancellationToken"/>.</param>.
        private void MiscellaneousEventsLoop(object tokenState)
        {
            var cancellationToken = (tokenState as CancellationToken?).Value;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Thread.Sleep here is OK because MiscellaneousEventsLoop runs on it's own thread.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                // TODO: update world light (daylight cycle).
                this.SampleServerMetrics();
            }
        }

        /// <summary>
        /// Tracks server metrics.
        /// </summary>
        private void SampleServerMetrics()
        {
            // Scheduler queue size.
            // this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.SchedulerQueueSizeMetricName).TrackValue(this.scheduler.QueueSize);

            // Online player count.
            // this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.OnlinePlayersMetricName).TrackValue(this.creatureManager.PlayerCount);
        }

        /// <summary>
        /// Dispatches an operation.
        /// </summary>
        /// <param name="operation">The operation to dispatch.</param>
        /// <param name="withDelay">Optional. A delay to dispatch the operation with.</param>
        private void DispatchOperation(IOperation operation, TimeSpan withDelay = default)
        {
            operation.ThrowIfNull(nameof(operation));

            // Normalize delay to protect against negative time spans.
            var operationDelay = withDelay < TimeSpan.Zero ? TimeSpan.Zero : withDelay;

            /*
            // Add delay if there is an associated exhaustion to this operation, if any.
            if (operation.RequestorId > 0 && operation.ExhaustionInfo.Any() && this.creatureManager.FindCreatureById(operation.RequestorId) is ICreature requestor)
            {
                // Delay by the maximum of any conditions needed to cool down.
                operationDelay += operation.ExhaustionInfo.Max(exhaustionInfo => requestor.RemainingExhaustionTime(exhaustionInfo.Key, this.scheduler.CurrentTime));
            }
            */

            this.scheduler.ScheduleEvent(operation, operationDelay);
        }

        /// <summary>
        /// Handles a signal from the scheduler that an event has been fired and begins processing it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The arguments of the event.</param>
        private void HandleSchedulerFiredEvent(object sender, EventFiredEventArgs eventArgs)
        {
            if (sender != this.scheduler || eventArgs?.Event == null)
            {
                return;
            }

            IEvent evt = eventArgs.Event;

            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                evt.Execute(this.PrepareContextForEvent(evt));

                sw.Stop();

                if (!evt.ExcludeFromTelemetry)
                {
                    // this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.ProcessedEventTimeMetricName, TelemetryConstants.EventTypeDimensionName).TrackValue(sw.ElapsedMilliseconds, evt.GetType().Name);
                }

                this.logger.LogTrace($"Handled {evt.GetType().Name} with id: {evt.EventId}, current game time: {this.scheduler.CurrentTime.ToUnixTimeMilliseconds()}.");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error in {evt.GetType().Name} with id: {evt.EventId}: {ex.Message}.");
                this.logger.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Prepares a new context for the given <see cref="IEvent"/>.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <returns>A new instance of the context prepared for the event.</returns>
        private IEventContext PrepareContextForEvent(IEvent evt)
        {
            evt.ThrowIfNull(nameof(evt));

            var eventType = evt.GetType();

            if (typeof(IOperation).IsAssignableFrom(eventType))
            {
                return new OperationContext(
                    this.logger,
                    this.applicationContext,
                    this.map,
                    this.creatureManager,
                    this.itemFactory,
                    this.creatureFactory,
                    this,
                    this.pathFinder,
                    this.predefinedItems,
                    this.scheduler);
            }

            return new EventContext(this.logger, () => this.scheduler.CurrentTime);
        }

        /// <summary>
        /// Handles a window loaded event from the map loader.
        /// </summary>
        /// <param name="fromX">The start X coordinate for the loaded window.</param>
        /// <param name="toX">The end X coordinate for the loaded window.</param>
        /// <param name="fromY">The start Y coordinate for the loaded window.</param>
        /// <param name="toY">The end Y coordinate for the loaded window.</param>
        /// <param name="fromZ">The start Z coordinate for the loaded window.</param>
        /// <param name="toZ">The end Z coordinate for the loaded window.</param>
        private void OnMapWindowLoaded(int fromX, int toX, int fromY, int toY, sbyte fromZ, sbyte toZ)
        {
            var x = 1 + Math.Abs(toX - fromX);
            var y = 1 + Math.Abs(toY - fromY);
            var z = 1 + Math.Abs(toZ - fromZ);

            // Track the loaded tiles in window.
            // this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.MapTilesLoadedMetricName).TrackValue(x * y * z);

            // For spawns, check which fall within this window:
            var spawnsInWindow = this.monsterSpawns
                .Where(s => s.Location.X >= fromX && s.Location.X <= toX &&
                            s.Location.Y >= fromY && s.Location.Y <= toY &&
                            s.Location.Z >= fromZ && s.Location.Z <= toZ);

            if (spawnsInWindow == null)
            {
                return;
            }

            foreach (var spawn in spawnsInWindow)
            {
                // this.DispatchOperation(new SpawnMonstersOperation(requestorId: 0, spawn));
            }
        }

        /// <summary>
        /// Handles commonly executed logic on any item being created.
        /// </summary>
        /// <param name="itemCreated">The item that was created.</param>
        private void AfterItemIsCreated(IItem itemCreated)
        {
            itemCreated.ThrowIfNull(nameof(itemCreated));

            this.logger.LogTrace($"Item created: {itemCreated.DescribeForLogger()}.");

            // this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.ItemCreatedMetricName).TrackValue(...);

            // Start decay for items that need it.
            // if (itemCreated.HasExpiration)
            // {
            //     this.AddOrAggregateCondition(itemCreated, new DecayingCondition(itemCreated), itemCreated.ExpirationTimeLeft);
            // }
        }
    }
}
