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

namespace Fibula.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Common.Contracts.Constants;
    using Fibula.Definitions.Constants;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Plugins.PathFinding.AStar.Extensions;
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Scheduling.Contracts.Delegates;
    using Fibula.Server.Contracts;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Contracts.Models;
    using Fibula.Server.Creatures;
    using Fibula.Server.Notifications;
    using Fibula.Server.Operations;
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
        /// Gets the tile factory instance.
        /// </summary>
        private readonly ITileFactory tileFactory;

        /// <summary>
        /// Gets the creature factory instance.
        /// </summary>
        private readonly ICreatureFactory creatureFactory;

        /// <summary>
        /// Gets the container manager in use.
        /// </summary>
        private readonly IContainerManager containerManager;

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
        /// <param name="containerManager">A reference to the container manager in use.</param>
        /// <param name="tileFactory">A reference to the tile factory in use.</param>
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
            IContainerManager containerManager,
            ITileFactory tileFactory,
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
            containerManager.ThrowIfNull(nameof(containerManager));
            tileFactory.ThrowIfNull(nameof(tileFactory));
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
            this.containerManager = containerManager;
            this.tileFactory = tileFactory;
            this.pathFinder = pathFinderAlgo;
            this.predefinedItems = predefinedItemSet;
            this.scheduler = scheduler;

            this.itemFactory.ItemCreated += this.AfterItemIsCreated;
            this.tileFactory.TileCreated += this.AfterTileIsCreated;

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
            this.tileFactory.TileCreated -= this.AfterTileIsCreated;

            // TODO: probably save game state here.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemType">The type of item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        /// <returns>True if the item is created successfully, false otherwise.</returns>
        public bool CreateItemAtLocation(Location location, ItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes)
        {
            if (itemType == null)
            {
                return false;
            }

            var attributesToSet = itemType.DefaultAttributes.Select(kvp => ((ItemAttribute)kvp.Key, kvp.Value));

            if (additionalAttributes != null)
            {
                attributesToSet = attributesToSet.Union(additionalAttributes);
            }

            var createItemOp = new CreateItemOperation(requestorId: 0, itemType.TypeId, location, attributesToSet.ToArray());

            createItemOp.Execute(this.PrepareContextForEvent(createItemOp));

            return true;
        }

        /// <summary>
        /// Creates item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemType">The type of item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        public void CreateItemAtLocationAsync(Location location, ItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes)
        {
            if (itemType == null)
            {
                return;
            }

            var attributesToSet = itemType.DefaultAttributes.Select(kvp => ((ItemAttribute)kvp.Key, kvp.Value));

            if (additionalAttributes != null)
            {
                attributesToSet = attributesToSet.Union(additionalAttributes);
            }

            var createItemOp = new CreateItemOperation(requestorId: 0, itemType.TypeId, location, attributesToSet.ToArray());

            this.DispatchOperation(createItemOp);
        }

        /// <summary>
        /// Performs creature speech asynchronously.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The type of channel of the speech.</param>
        /// <param name="content">The content of the speech.</param>
        /// <param name="receiver">Optional. The receiver of the speech, if any.</param>
        public void DoCreatureSpeechAsync(uint creatureId, SpeechType speechType, ChatChannelType channelType, string content, string receiver = "")
        {
            var speechOp = new SpeechOperation(creatureId, speechType, channelType, content, receiver);

            this.DispatchOperation(speechOp);
        }

        /// <summary>
        /// Performs a creature turn asynchronously.
        /// </summary>
        /// <param name="requestorId">The id of the creature.</param>
        /// <param name="creatureId">The id of the creature to turn.</param>
        /// <param name="direction">The direction to turn to.</param>
        public void DoCreatureTurnAsync(uint requestorId, uint creatureId, Direction direction)
        {
            var creature = this.creatureManager.FindCreatureById(creatureId);
            var turnToDirOp = new TurnToDirectionOperation(creature, direction);

            this.DispatchOperation(turnToDirOp);
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
        /// <param name="playerId">The id of the player to log out.</param>
        public void LogPlayerOut(uint playerId)
        {
            var player = this.creatureManager.FindPlayerById(playerId);

            if (player == null)
            {
                return;
            }

            var logOutOp = new LogOutOperation(player.Id, player);

            this.DispatchOperation(logOutOp);
        }

        /// <summary>
        /// Cancels all operations that a player has pending, immediately.
        /// </summary>
        /// <param name="playerId">The id of the player to cancel operations for.</param>
        /// <param name="category">Optional. The specific category of operations to cancel. All operations are cancelled if no type is specified.</param>
        public void CancelPlayerOperations(uint playerId, OperationCategory category = OperationCategory.Any)
        {
            if (!(this.creatureManager.FindPlayerById(playerId) is Player player))
            {
                return;
            }

            Type typeOfOperationToCancel = category switch
            {
                OperationCategory.Movement => typeof(MovementOperation),
                _ => typeof(IOperation),
            };

            var cancelOp = new CancelOperationsOperation(player.Id, player, typeOfOperationToCancel);

            cancelOp.Execute(this.PrepareContextForEvent(cancelOp));
        }

        /// <summary>
        /// Cancels all operations that a player has pending asynchronously.
        /// </summary>
        /// <param name="playerId">The id of the player to cancel operations for.</param>
        /// <param name="category">Optional. The specific category of operations to cancel. All operations are cancelled if no type is specified.</param>
        public void CancelPlayerOperationsAsync(uint playerId, OperationCategory category = OperationCategory.Any)
        {
            if (!(this.creatureManager.FindPlayerById(playerId) is Player player))
            {
                return;
            }

            Type typeOfOperationToCancel = category switch
            {
                OperationCategory.Movement => typeof(MovementOperation),
                _ => typeof(IOperation),
            };

            var cancelOp = new CancelOperationsOperation(player.Id, player, typeOfOperationToCancel);

            this.DispatchOperation(cancelOp);
        }

        /// <summary>
        /// Describes a thing for a player that is looking at it.
        /// </summary>
        /// <param name="thingId">The id of the thing to describe.</param>
        /// <param name="location">The location of the thing to describe.</param>
        /// <param name="stackPosition">The position in the stack within the location of the thing to describe.</param>
        /// <param name="playerId">The player for which to describe the thing for.</param>
        public void DescribeThingAt(ushort thingId, Location location, byte stackPosition, uint playerId)
        {
            var player = this.creatureManager.FindPlayerById(playerId);

            if (player == null)
            {
                return;
            }

            var lootAtOp = new LookAtOperation(thingId, location, stackPosition, player);

            this.DispatchOperation(lootAtOp);
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

            creature.LocationChanged += this.OnThingLocationChanged;

            if (creature is ICombatant combatant)
            {
                combatant.StatChanged += this.OnCreatureStatChanged;

                combatant.Death += this.OnCombatantDeath;
                combatant.AttackTargetChanged += this.CombatantAttackTargetChanged;
                combatant.FollowTargetChanged += this.CreatureFollowTargetChanged;
                combatant.LocationChanged += this.OnThingLocationChanged;

                // As a creature that can sense.
                combatant.CreatureSeen += this.CreatureHasSeenCreature;
                combatant.CreatureLost += this.CreatureHasLostCreature;

                // As a skilled creature
                combatant.SkillChanged += this.SkilledCreatureSkillChanged;

                // Find now-spectators of this creature to start tracking that guy.
                foreach (var spectator in this.map.FindCreaturesThatCanSee(creature.Location))
                {
                    if (spectator is ICombatant combatantSpectator && spectator.CanSee(creature))
                    {
                        combatant.PerceiveCreature(combatantSpectator);
                        combatantSpectator.PerceiveCreature(combatant);
                    }
                }
            }

            this.logger.LogDebug($"Placed {creature.Name} at {targetTile.Location}.");

            var placedAtIndex = targetTile.GetIndexOfThing(creature);

            if (placedAtIndex != byte.MaxValue)
            {
                var player = creature as IPlayer;

                if (player != null)
                {
                    var loginNotification = new PlayerLoginNotification(player, creature.Location, this.map.DescribeAt(player, player.Location), AnimatedEffect.BubbleBlue);

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

            creature.LocationChanged -= this.OnThingLocationChanged;

            if (creature is ICombatant combatant)
            {
                combatant.SetAttackTarget(null);

                foreach (var attacker in combatant.AttackedBy)
                {
                    attacker.LostCreature(combatant);
                }

                foreach (var trackedCreature in combatant.PerceivedCreatures)
                {
                    combatant.LostCreature(trackedCreature);

                    if (trackedCreature is ISentientCreature sentientCreature)
                    {
                        sentientCreature.LostCreature(combatant);
                    }
                }

                combatant.StatChanged -= this.OnCreatureStatChanged;

                combatant.Death -= this.OnCombatantDeath;
                combatant.AttackTargetChanged -= this.CombatantAttackTargetChanged;
                combatant.FollowTargetChanged -= this.CreatureFollowTargetChanged;

                // As a creature that can sense.
                combatant.CreatureSeen -= this.CreatureHasSeenCreature;
                combatant.CreatureLost -= this.CreatureHasLostCreature;

                // As a skilled creature
                combatant.SkillChanged -= this.SkilledCreatureSkillChanged;
            }

            var spectators = (creature is IPlayer player) ? this.map.FindPlayersThatCanSee(creature.Location).Union(player.YieldSingleItem()) : this.map.FindPlayersThatCanSee(creature.Location);
            var notification = new CreatureRemovedNotification(spectators, creature, oldStackpos);

            this.SendNotification(notification);

            return true;
        }

        /// <summary>
        /// Places a new monster of the given race, at the given location.
        /// </summary>
        /// <param name="raceId">The id of race of monster to place.</param>
        /// <param name="location">The location at which to place the monster.</param>
        public void PlaceNewMonsterAtAsync(string raceId, Location location)
        {
            using var uow = this.applicationContext.CreateNewUnitOfWork();

            var monsterType = uow.MonsterTypes.GetByRaceId(raceId);

            if (monsterType == null)
            {
                this.logger.LogWarning($"Unable to place monster. Could not find a monster with the id {raceId} in the repository. ({nameof(this.PlaceNewMonsterAtAsync)})");

                return;
            }

            var newMonster = this.creatureFactory.CreateCreature(new CreatureCreationArguments() { Type = CreatureType.Monster, Metadata = monsterType }) as IMonster;

            if (!this.map.HasTileAt(location, out ITile targetTile) || targetTile.IsPathBlocking())
            {
                return;
            }

            var placeMonsterOp = new PlaceCreatureOperation(requestorId: 0, targetTile, newMonster);

            this.DispatchOperation(placeMonsterOp);
        }

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creatureId">The id of the creature to reset the walk plan of.</param>
        /// <param name="directions">The directions for the new plan.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        public void ResetCreatureWalkPlan(uint creatureId, Direction[] directions, WalkPlanStrategy strategy = WalkPlanStrategy.DoNotRecalculate)
        {
            if (!(this.creatureManager.FindCreatureById(creatureId) is Creature creature) || !directions.Any())
            {
                return;
            }

            var lastLoc = creature.Location;
            var waypoints = new List<Location>()
            {
                creature.Location,
            };

            // Build the waypoints.
            for (int i = 0; i < directions.Length; i++)
            {
                var nextLoc = lastLoc.LocationAt(directions[i]);

                waypoints.Add(nextLoc);
                lastLoc = nextLoc;
            }

            creature.WalkPlan = new WalkPlan(strategy, () => lastLoc, goalDistance: 0, waypoints.ToArray());

            this.scheduler.CancelAllFor(creature.Id, typeof(AutoWalkOrchestratorOperation));

            var autoWalkOrchOp = new AutoWalkOrchestratorOperation(creature);

            this.DispatchOperation(autoWalkOrchOp);
        }

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creatureId">The id of the creature to reset the walk plan of.</param>
        /// <param name="targetCreature">The creature towards which the walk plan will be generated to.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        /// <param name="targetDistance">Optional. The target distance to calculate from the target creature.</param>
        /// <param name="excludeCurrentLocation">Optional. A value indicating whether to exclude the current creature's location from being the goal location.</param>
        public void ResetCreatureWalkPlan(uint creatureId, ICreature targetCreature, WalkPlanStrategy strategy = WalkPlanStrategy.ConservativeRecalculation, int targetDistance = 1, bool excludeCurrentLocation = false)
        {
            if (!(this.creatureManager.FindCreatureById(creatureId) is Creature creature))
            {
                return;
            }

            this.scheduler.CancelAllFor(creature.Id, typeof(AutoWalkOrchestratorOperation));

            if (targetCreature == null)
            {
                return;
            }

            var (result, endLocation, directions) = excludeCurrentLocation ?
                this.pathFinder.FindPathBetween(creature.Location, targetCreature.Location, creature, targetDistance, excludeLocations: creature.Location)
                :
                this.pathFinder.FindPathBetween(creature.Location, targetCreature.Location, creature, targetDistance);

            var waypoints = new List<Location>()
            {
                creature.Location,
            };

            var lastLoc = creature.Location;

            // Calculate and add the waypoints.
            foreach (var dir in directions)
            {
                var nextLoc = lastLoc.LocationAt(dir);

                waypoints.Add(nextLoc);

                lastLoc = nextLoc;
            }

            creature.WalkPlan = new WalkPlan(strategy, () => targetCreature.Location, targetDistance, waypoints.ToArray());

            var autoWalkOrchOp = new AutoWalkOrchestratorOperation(creature);

            this.DispatchOperation(autoWalkOrchOp);
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
        /// Sets the fight, chase and safety modes of a combatant.
        /// </summary>
        /// <param name="combatantId">The id of the combatant that updated modes.</param>
        /// <param name="fightMode">The fight mode to change to.</param>
        /// <param name="chaseMode">The chase mode to change to.</param>
        /// <param name="safeModeOn">A value indicating whether the attack safety lock is on.</param>
        public void SetCombatantModes(uint combatantId, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn)
        {
            if (!(this.creatureManager.FindCreatureById(combatantId) is ICombatant combatant))
            {
                return;
            }

            var changeModesOp = new ChangeModesOperation(combatant.Id, fightMode, chaseMode, safeModeOn);

            this.DispatchOperation(changeModesOp);
        }

        /// <summary>
        /// Re-sets the attack target of the attacker and it's (possibly new) target.
        /// </summary>
        /// <param name="attackerId">The id of the attacker.</param>
        /// <param name="targetId">The id of the new target, which can be null.</param>
        public void SetCombatantAttackTarget(uint attackerId, uint targetId)
        {
            if (!(this.creatureManager.FindCreatureById(attackerId) is ICombatant attacker))
            {
                return;
            }

            attacker.SetAttackTarget(this.creatureManager.FindCreatureById(targetId) as ICombatant);
        }

        /// <summary>
        /// Re-sets the follow target of the combatant and it's (possibly new) target.
        /// </summary>
        /// <param name="followerId">The id of the follower.</param>
        /// <param name="targetId">The id of the new target, which can be null.</param>
        public void SetCombatantFollowTarget(uint followerId, uint targetId)
        {
            if (!(this.creatureManager.FindCreatureById(followerId) is ICombatant follower))
            {
                return;
            }

            follower.SetFollowTarget(this.creatureManager.FindCreatureById(targetId) as ICombatant);
        }

        /// <summary>
        /// Handles the aftermath of a location change from a thing.
        /// </summary>
        /// <param name="thing">The thing which's location changed.</param>
        /// <param name="previousLocation">The previous location of the thing.</param>
        private void OnThingLocationChanged(IThing thing, Location previousLocation)
        {
            thing.ThrowIfNull(nameof(thing));

            this.logger.LogTrace($"{thing} changed location from {previousLocation} to {thing.Location}.");

            thing.ThrowIfNull(nameof(thing));

            if (thing is IPlayer player)
            {
                // If the creature is a player, we must check if the movement caused it to walk away from any open containers.
                foreach (var container in this.containerManager.FindAllForCreature(player.Id))
                {
                    if (container == null)
                    {
                        continue;
                    }

                    var locationDiff = container.Location - player.Location;

                    if (locationDiff.MaxValueIn2D > 1 || locationDiff.Z != 0)
                    {
                        var containerId = this.containerManager.FindForCreature(player.Id, container);

                        if (containerId != ItemConstants.UnsetContainerPosition)
                        {
                            this.containerManager.CloseContainer(player.Id, container, containerId);
                        }
                    }
                }
            }

            if (thing is ICombatant movingCombatant)
            {
                // Check if we are in range to perform the attack operation, if any.
                // Do the same for the creatures attacking it, in case the movement caused it to walk into the range of them.
                foreach (var combatant in movingCombatant.YieldSingleItem().Union(movingCombatant.AttackedBy))
                {
                    if (!combatant.TryRetrieveTrackedOperation(nameof(BasicAttackOperation), out IOperation operation) || !(operation is BasicAttackOperation basicAttackOp))
                    {
                        continue;
                    }

                    if (basicAttackOp.Target != movingCombatant && basicAttackOp.Attacker != movingCombatant)
                    {
                        continue;
                    }

                    var distanceBetweenCombatants = (basicAttackOp.Attacker?.Location ?? basicAttackOp.Target.Location) - basicAttackOp.Target.Location;
                    var inRange = distanceBetweenCombatants.MaxValueIn2D <= basicAttackOp.Attacker.AutoAttackRange && distanceBetweenCombatants.Z == 0;

                    if (inRange)
                    {
                        // Expedite their orchestration operation instead of the actual basic attack operation.
                        // This will figure out the next attack's right delay amount.
                        if (combatant.TryRetrieveTrackedOperation(nameof(AttackOrchestratorOperation), out IOperation atkOrchestrationOp))
                        {
                            atkOrchestrationOp.Expedite();
                        }
                    }
                }

                // And check if it walked into new combatants view range.
                var spectatorsAtDestination = this.map.FindCreaturesThatCanSee(thing.Location).OfType<ICombatant>();
                var spectatorsAtSource = this.map.FindCreaturesThatCanSee(previousLocation).OfType<ICombatant>();

                var spectatorsLost = spectatorsAtSource.Except(spectatorsAtDestination);

                // Make new spectators aware that this creature moved into their view.
                foreach (var spectator in spectatorsAtDestination)
                {
                    spectator.PerceiveCreature(movingCombatant);
                    movingCombatant.PerceiveCreature(spectator);
                }

                // Now make old spectators aware that this creature moved out of their view.
                foreach (var spectator in spectatorsLost)
                {
                    spectator.LostCreature(movingCombatant);
                    movingCombatant.LostCreature(spectator);
                }
            }

            /* TODO: event rules.
            context.EventRulesApi.EvaluateRules(this, EventRuleType.Separation, new SeparationEventRuleArguments(fromTile.Location, creature, requestorCreature));
            context.EventRulesApi.EvaluateRules(this, EventRuleType.Collision, new CollisionEventRuleArguments(toTile.Location, creature, requestorCreature));
            context.EventRulesApi.EvaluateRules(this, EventRuleType.Movement, new MovementEventRuleArguments(creature, requestorCreature));
            */
        }

        /// <summary>
        /// Handles a stat change event from a creature.
        /// </summary>
        /// <param name="creature">The creature for which the stat changed.</param>
        /// <param name="statThatChanged">The stat that changed.</param>
        /// <param name="previousValue">The previous stat value.</param>
        /// <param name="previousPercent">The previous percent for the stat.</param>
        private void OnCreatureStatChanged(ICreature creature, IStat statThatChanged, uint previousValue, byte previousPercent)
        {
            creature.ThrowIfNull(nameof(creature));
            statThatChanged.ThrowIfNull(nameof(statThatChanged));

            if (statThatChanged.Type == CreatureStat.HitPoints)
            {
                // This updates the health as seen by others.
                this.SendNotification(new CreatureHealthUpdateNotification(this.map.FindPlayersThatCanSee(creature.Location), creature));
            }

            if (creature is IPlayer player)
            {
                if (statThatChanged.Type == CreatureStat.BaseSpeed)
                {
                    this.SendNotification(new CreatureSpeedChangeNotification(this.map.FindPlayersThatCanSee(creature.Location), player));
                }

                // And this updates the health of the player.
                this.SendNotification(new PlayerStatsUpdateNotification(player));
            }
        }

        /// <summary>
        /// Handles the death of a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that died.</param>
        private void OnCombatantDeath(ICombatant combatant)
        {
            this.CancelPlayerOperationsAsync(combatant.Id);

            var rng = new Random();
            var deathDelay = TimeSpan.FromMilliseconds(rng.Next(MechanicsConstants.DefaultDeathDelayMs));
            var deathOp = new DeathOperation(requestorId: 0, combatant);

            this.DispatchOperation(deathOp, deathDelay);
        }

        /// <summary>
        /// Handles an attack target change from a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that died.</param>
        /// <param name="oldTarget">The previous attack target, which can be null.</param>
        private void CombatantAttackTargetChanged(ICombatant combatant, ICombatant oldTarget)
        {
            if (combatant == null)
            {
                return;
            }

            this.scheduler.CancelAllFor(combatant.Id, typeof(AttackOrchestratorOperation));

            if (combatant.AutoAttackTarget != null)
            {
                var autoAttackOrchestrationOp = new AttackOrchestratorOperation(combatant);

                this.DispatchOperation(autoAttackOrchestrationOp);

                // TODO: in-fight conditions for both here.
            }
        }

        /// <summary>
        /// Handles a follow target change from a combatant.
        /// </summary>
        /// <param name="combatant">The creature that changed follow target.</param>
        /// <param name="oldTarget">The old follow target, if any.</param>
        private void CreatureFollowTargetChanged(ICombatant combatant, ICreature oldTarget)
        {
            if (combatant == null)
            {
                return;
            }

            if (combatant.ChaseTarget != oldTarget)
            {
                this.ResetCreatureWalkPlan(combatant.Id, combatant.ChaseTarget, targetDistance: combatant.AutoAttackRange);
            }
        }

        /// <summary>
        /// Handles the event when a creature has seen another creature.
        /// </summary>
        /// <param name="creature">The monster that sees the other.</param>
        /// <param name="creatureSeen">The creature that was seen.</param>
        private void CreatureHasSeenCreature(ISentientCreature creature, ICreature creatureSeen)
        {
            creature.ThrowIfNull(nameof(creature));
            creatureSeen.ThrowIfNull(nameof(creatureSeen));

            if (creature is ICombatant combatant && creatureSeen is ICombatant combatantSeen)
            {
                combatant.AddToCombatList(combatantSeen);
            }
        }

        /// <summary>
        /// Handles the event when a creature has lost another creature.
        /// </summary>
        /// <param name="creature">The monster that sees the other.</param>
        /// <param name="creatureLost">The creature that was lost.</param>
        private void CreatureHasLostCreature(ISentientCreature creature, ICreature creatureLost)
        {
            creature.ThrowIfNull(nameof(creature));
            creatureLost.ThrowIfNull(nameof(creatureLost));

            if (creature is ICombatant combatant && creatureLost is ICombatant combatantLost)
            {
                combatant.RemoveFromCombatList(combatantLost);
            }
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
            this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.SchedulerQueueSizeMetricName).TrackValue(this.scheduler.QueueSize);

            // Online player count.
            this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.OnlinePlayersMetricName).TrackValue(this.creatureManager.PlayerCount);
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

            // Add delay if there is an associated exhaustion to this operation, if any.
            if (operation.RequestorId > 0 && operation.ExhaustionInfo.Any() && this.creatureManager.FindCreatureById(operation.RequestorId) is ICreature requestor)
            {
                // Delay by the maximum of any conditions needed to cool down.
                operationDelay += operation.ExhaustionInfo.Max(exhaustionInfo => requestor.RemainingExhaustionTime(exhaustionInfo.Key, this.scheduler.CurrentTime));
            }

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
                    this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.ProcessedEventTimeMetricName, TelemetryConstants.EventTypeDimensionName).TrackValue(sw.ElapsedMilliseconds, evt.GetType().Name);
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
                    this.containerManager,
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
            this.applicationContext.TelemetryClient.GetMetric(TelemetryConstants.MapTilesLoadedMetricName).TrackValue(x * y * z);

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
                this.DispatchOperation(new SpawnMonstersOperation(requestorId: 0, spawn));
            }
        }

        /// <summary>
        /// Handles a skill level change from a skilled creature.
        /// </summary>
        /// <param name="skilledCreature">The skilled creature for which the skill changed.</param>
        /// <param name="skillThatChanged">The skill that changed.</param>
        /// <param name="previousLevel">The previous skill level.</param>
        /// <param name="previousPercent">The previous percent of completion to next level.</param>
        /// <param name="countDelta">Optional. The delta in the count for this skill. Not always sent.</param>
        private void SkilledCreatureSkillChanged(ISkilledCreature skilledCreature, ISkill skillThatChanged, uint previousLevel, byte previousPercent, long? countDelta = null)
        {
            if (skilledCreature == null || skillThatChanged == null)
            {
                this.logger.LogWarning($"Null {nameof(skilledCreature)} or {nameof(skillThatChanged)} in {nameof(this.SkilledCreatureSkillChanged)}, ignoring...");

                return;
            }

            if (!(skilledCreature is IPlayer player))
            {
                return;
            }

            // Send the advancement or regression text.
            if (skillThatChanged.CurrentLevel != previousLevel)
            {
                bool isAdvance = skillThatChanged.CurrentLevel > previousLevel;

                var nameToLog = string.IsNullOrWhiteSpace(player.Article) ? player.Name : $"{player.Article} {player.Name}";

                this.logger.LogDebug($"{nameToLog} {(isAdvance ? "advanced" : "regressed")} from {previousLevel} to {skillThatChanged.CurrentLevel} in skill {skillThatChanged.Type}.");

                var skillChangeNotificationMessage = skillThatChanged.Type switch
                {
                    SkillType.Experience => $"You {(isAdvance ? "advanced" : "were downgraded")} from level {previousLevel} to level {skillThatChanged.CurrentLevel}.",
                    SkillType.Magic => $"You advanced to magic level {skillThatChanged.CurrentLevel}.",
                    _ => isAdvance ? $"You advanced in {skillThatChanged.Type.ToFriendlySkillName()}." : string.Empty,
                };

                // Send the message only if we actually have text to send.
                if (!string.IsNullOrWhiteSpace(skillChangeNotificationMessage))
                {
                    var notification = new TextMessageNotification(player.YieldSingleItem(), MessageType.CenterWhite, skillChangeNotificationMessage);

                    this.NotificationReady?.Invoke(this, notification);
                }

                // Increase the creature's stats.
                if (skillThatChanged.Type == SkillType.Experience)
                {
                    var changeFactor = (int)((long)skillThatChanged.CurrentLevel - previousLevel);

                    if (changeFactor > 0)
                    {
                        player.Stats[CreatureStat.HitPoints].IncreaseMaximum(15 * changeFactor);
                        player.Stats[CreatureStat.ManaPoints].IncreaseMaximum(15 * changeFactor);
                        player.Stats[CreatureStat.CarryStrength].IncreaseMaximum(30 * changeFactor);
                    }
                    else
                    {
                        player.Stats[CreatureStat.HitPoints].DecreaseMaximum(15 * changeFactor);
                        player.Stats[CreatureStat.ManaPoints].DecreaseMaximum(15 * changeFactor);
                        player.Stats[CreatureStat.CarryStrength].DecreaseMaximum(30 * changeFactor);
                    }

                    // And send the updated speed.
                    player.Stats[CreatureStat.BaseSpeed].Set(220 + (2 * (skillThatChanged.CurrentLevel - 1)));
                }
            }

            if (skillThatChanged.Percent != previousPercent || skillThatChanged.Type == SkillType.Experience)
            {
                // Send the new percentual value to the player.
                var playerSkillsUpdateNotification = new PlayerSkillsUpdateNotification(player);
                var playerStatsUpdateNotification = new PlayerStatsUpdateNotification(player);

                this.SendNotification(playerSkillsUpdateNotification);
                this.SendNotification(playerStatsUpdateNotification);

                // Add the animated text for experience.
                if (skillThatChanged.Type == SkillType.Experience && countDelta.HasValue && countDelta.Value > 0)
                {
                    var notification = new AnimatedTextNotification(this.map.FindPlayersThatCanSee(player.Location), player.Location, TextColor.White, countDelta.Value.ToString());

                    this.NotificationReady?.Invoke(this, notification);
                }
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

            // TODO: Start decay for items that need it.
        }

        /// <summary>
        /// Handles commonly executed logic on any tile being created.
        /// </summary>
        /// <param name="tileCreated">The tile that was created.</param>
        private void AfterTileIsCreated(ITile tileCreated)
        {
        }
    }
}
