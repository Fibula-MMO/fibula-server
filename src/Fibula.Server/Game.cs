// -----------------------------------------------------------------
// <copyright file="Game.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Common.Contracts.Constants;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Constants;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Definitions.Flags;
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Scheduling.Contracts.Delegates;
    using Fibula.Server.Contracts;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Constants;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Creatures;
    using Fibula.Server.Mechanics.Conditions;
    using Fibula.Server.Mechanics.Notifications;
    using Fibula.Server.Mechanics.Operations;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents the game instance.
    /// </summary>
    public class Game : IGame
    {
        /// <summary>
        /// Defines the <see cref="TimeSpan"/> to wait between checks for idle players and connections.
        /// </summary>
        private static readonly TimeSpan IdleCheckDelay = TimeSpan.FromSeconds(30);

        /// <summary>
        /// A reference to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Stores the application context.
        /// </summary>
        private readonly IApplicationContext applicationContext;

        /// <summary>
        /// Stores the scheduler used by the game.
        /// </summary>
        private readonly IScheduler scheduler;

        /// <summary>
        /// The current map descriptor.
        /// </summary>
        private readonly IMapDescriptor mapDescriptor;

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
        /// The container manager instance.
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
        /// The predefined item set declared.
        /// </summary>
        private readonly IPredefinedItemSet predefinedItemSet;

        /// <summary>
        /// Stores the world information.
        /// </summary>
        private readonly WorldInformation worldInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="applicationContext">A reference to the application context.</param>
        /// <param name="mapDescriptor">A reference to the map descriptor in use.</param>
        /// <param name="map">A reference to the map.</param>
        /// <param name="creatureManager">A reference to the creature manager in use.</param>
        /// <param name="itemFactory">A reference to the item factory in use.</param>
        /// <param name="creatureFactory">A reference to the creature factory in use.</param>
        /// <param name="containerManager">A reference to the container manager in use.</param>
        /// <param name="pathFinderAlgo">A reference to the path finding algorithm in use.</param>
        /// <param name="predefinedItemSet">A reference to the predefined item set declared.</param>
        /// <param name="monsterSpawnsLoader">A reference to the monster spawns loader.</param>
        /// <param name="scheduler">A reference to the global scheduler instance.</param>
        public Game(
            ILogger<Game> logger,
            IApplicationContext applicationContext,
            IMapDescriptor mapDescriptor,
            IMap map,
            ICreatureManager creatureManager,
            IItemFactory itemFactory,
            ICreatureFactory creatureFactory,
            IContainerManager containerManager,
            IPathFinder pathFinderAlgo,
            IPredefinedItemSet predefinedItemSet,
            IMonsterSpawnLoader monsterSpawnsLoader,
            IScheduler scheduler)
        {
            logger.ThrowIfNull(nameof(logger));
            applicationContext.ThrowIfNull(nameof(applicationContext));
            mapDescriptor.ThrowIfNull(nameof(mapDescriptor));
            map.ThrowIfNull(nameof(map));
            creatureManager.ThrowIfNull(nameof(creatureManager));
            itemFactory.ThrowIfNull(nameof(itemFactory));
            creatureFactory.ThrowIfNull(nameof(creatureFactory));
            containerManager.ThrowIfNull(nameof(containerManager));
            pathFinderAlgo.ThrowIfNull(nameof(pathFinderAlgo));
            predefinedItemSet.ThrowIfNull(nameof(predefinedItemSet));
            monsterSpawnsLoader.ThrowIfNull(nameof(monsterSpawnsLoader));
            scheduler.ThrowIfNull(nameof(scheduler));

            this.logger = logger;
            this.applicationContext = applicationContext;
            this.mapDescriptor = mapDescriptor;
            this.map = map;
            this.creatureManager = creatureManager;
            this.itemFactory = itemFactory;
            this.creatureFactory = creatureFactory;
            this.containerManager = containerManager;
            this.pathFinder = pathFinderAlgo;
            this.predefinedItemSet = predefinedItemSet;
            this.scheduler = scheduler;

            // Initialize game vars.
            this.worldInfo = new WorldInformation()
            {
                Status = WorldState.Loading,
                LightColor = LightConstants.WhiteColor,
                LightLevel = LightConstants.WorldDay,
            };

            this.itemFactory.ItemCreated += this.AfterItemIsCreated;

            // Load the spawns
            this.monsterSpawns = monsterSpawnsLoader.LoadSpawns();

            // Hook some event handlers.
            this.scheduler.EventFired += this.ProcessFiredEvent;
            this.map.WindowLoaded += this.OnMapWindowLoaded;
        }

        /// <summary>
        /// Gets the game world's information.
        /// </summary>
        public IWorldInformation WorldInfo => this.worldInfo;

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
                    var connectionSweepperTask = Task.Factory.StartNew(this.IdlePlayerSweep, cancellationToken, TaskCreationOptions.LongRunning);
                    var miscellaneusEventsTask = Task.Factory.StartNew(this.MiscellaneousEventsLoop, cancellationToken, TaskCreationOptions.LongRunning);

                    // start the scheduler.
                    var schedulerTask = this.scheduler.RunAsync(cancellationToken);

                    // Open the game world!
                    this.worldInfo.Status = WorldState.Open;

                    await Task.WhenAll(connectionSweepperTask, miscellaneusEventsTask, schedulerTask);
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
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        public IOperation CreateItemAtLocationAsync(Location location, ItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes)
        {
            if (itemType == null)
            {
                return null;
            }

            var attributesToSet = itemType.DefaultAttributes.Select(kvp => ((ItemAttribute)kvp.Key, kvp.Value));

            if (additionalAttributes != null)
            {
                attributesToSet = attributesToSet.Union(additionalAttributes);
            }

            var createItemOp = new CreateItemOperation(requestorId: 0, itemType.TypeId, location, attributesToSet.ToArray());

            this.DispatchOperation(createItemOp);

            return createItemOp;
        }

        /// <summary>
        /// Creates a new item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemTypeId">The type id of the item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        public IOperation CreateItemAtLocationAsync(Location location, ushort itemTypeId, params (ItemAttribute, IConvertible)[] additionalAttributes)
        {
            var createItemOp = new CreateItemOperation(requestorId: 0, itemTypeId, location, additionalAttributes);

            this.DispatchOperation(createItemOp);

            return createItemOp;
        }

        /// <summary>
        /// Adds or aggregates a condition to an afflicted thing.
        /// </summary>
        /// <param name="thing">The thing to check the conditions on.</param>
        /// <param name="condition">The condition to add or extend.</param>
        /// <param name="duration">The duration for the condition being added.</param>
        public void AddOrAggregateCondition(IThing thing, ICondition condition, TimeSpan duration)
        {
            thing.ThrowIfNull(nameof(thing));
            condition.ThrowIfNull(nameof(condition));

            if (!thing.TrackedEvents.TryGetValue(condition.EventType, out IEvent existingEvent))
            {
                thing.StartTrackingEvent(condition);

                this.scheduler.ScheduleEvent(condition, duration, scheduleAsync: true);

                this.logger.LogTrace($"Added {condition.GetType().Name} to {thing.DescribeForLogger()}.");

                if (thing is IPlayer player)
                {
                    this.scheduler.ScheduleEvent(new GenericNotification(() => player.YieldSingleItem(), new PlayerConditionsPacket(player)), scheduleAsync: true);
                }

                return;
            }

            if (existingEvent is ICondition existingCondition && existingCondition.Type == condition.Type)
            {
                if (existingCondition.Aggregate(condition))
                {
                    // Calculate the delay that we need to apply to the condition.
                    var existingConditionTimeToFire = this.scheduler.CalculateTimeToFire(existingCondition);
                    var extraTimeNeeded = duration > existingConditionTimeToFire ? duration - existingConditionTimeToFire : TimeSpan.Zero;

                    if (extraTimeNeeded > TimeSpan.Zero)
                    {
                        existingCondition.Delay(extraTimeNeeded);
                    }
                }
            }
        }

        /// <summary>
        /// Re-sets the attack target of the attacker and it's (possibly new) target.
        /// </summary>
        /// <param name="attacker">The attacker.</param>
        /// <param name="target">The new target.</param>
        public void SetCombatantAttackTarget(ICombatant attacker, ICombatant target)
        {
            if (attacker == null)
            {
                return;
            }

            attacker.SetAttackTarget(target);
        }

        /// <summary>
        /// Re-sets the follow target of the combatant and it's (possibly new) target.
        /// </summary>
        /// <param name="combatant">The attacker.</param>
        /// <param name="target">The new target.</param>
        public void SetCombatantFollowTarget(ICombatant combatant, ICombatant target)
        {
            if (combatant == null)
            {
                return;
            }

            combatant.SetFollowTarget(target);
        }

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creature">The creature to reset the walk plan of.</param>
        /// <param name="directions">The directions for the new plan.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        public void ResetCreatureWalkPlan(ICreature creature, Direction[] directions, WalkPlanStrategy strategy = WalkPlanStrategy.DoNotRecalculate)
        {
            if (creature == null || !directions.Any())
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
        /// <param name="creature">The creature to reset the walk plan of.</param>
        /// <param name="targetCreature">The creature towards which the walk plan will be generated to.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        /// <param name="targetDistance">Optional. The target distance to calculate from the target creature.</param>
        /// <param name="excludeCurrentLocation">Optional. A value indicating whether to exclude the current creature's location from being the goal location.</param>
        public void ResetCreatureWalkPlan(ICreature creature, ICreature targetCreature, WalkPlanStrategy strategy = WalkPlanStrategy.ConservativeRecalculation, int targetDistance = 1, bool excludeCurrentLocation = false)
        {
            if (creature == null)
            {
                return;
            }

            this.scheduler.CancelAllFor(creature.Id, typeof(AutoWalkOrchestratorOperation));

            if (targetCreature == null)
            {
                return;
            }

            var (result, endLocation, directions) = excludeCurrentLocation ?
                this.pathFinder.FindBetween(creature.Location, targetCreature.Location, creature, targetDistance, excludeLocations: creature.Location)
                :
                this.pathFinder.FindBetween(creature.Location, targetCreature.Location, creature, targetDistance);

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
        /// Cancels all operations that a player has pending, immediately.
        /// </summary>
        /// <param name="player">The player to cancel operations for.</param>
        /// <param name="typeOfOperationToCancel">Optional. The specific type of operation to cancel. All operations are cancelled if no type is specified.</param>
        public void CancelPlayerOperations(IPlayer player, Type typeOfOperationToCancel = null)
        {
            if (player == null || (typeOfOperationToCancel != null && !typeof(IOperation).IsAssignableFrom(typeOfOperationToCancel)))
            {
                return;
            }

            var cancelOp = new CancelOperationsOperation(player.Id, player, typeOfOperationToCancel);

            cancelOp.Execute(this.PrepareContextForEvent(cancelOp));
        }

        /// <summary>
        /// Cancels all operations that a player has pending asynchronously.
        /// </summary>
        /// <param name="player">The player to cancel operations for.</param>
        /// <param name="typeOfOperationToCancel">Optional. The specific type of operation to cancel. All operations are cancelled if no type is specified.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        public IOperation CancelPlayerOperationsAsync(IPlayer player, Type typeOfOperationToCancel = null)
        {
            if (player == null || (typeOfOperationToCancel != null && !typeof(IOperation).IsAssignableFrom(typeOfOperationToCancel)))
            {
                return null;
            }

            var cancelOp = new CancelOperationsOperation(player.Id, player, typeOfOperationToCancel);

            this.DispatchOperation(cancelOp);

            return cancelOp;
        }

        /// <summary>
        /// Sets the fight, chase and safety modes of a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that update modes.</param>
        /// <param name="fightMode">The fight mode to change to.</param>
        /// <param name="chaseMode">The chase mode to change to.</param>
        /// <param name="safeModeOn">A value indicating whether the attack safety lock is on.</param>
        public void SetCombatantModes(ICombatant combatant, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn)
        {
            if (combatant == null)
            {
                return;
            }

            var changeModesOp = new ChangeModesOperation(combatant.Id, fightMode, chaseMode, safeModeOn);

            this.DispatchOperation(changeModesOp);
        }

        /// <summary>
        /// Performs creature speech asynchronously.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The type of channel of the speech.</param>
        /// <param name="content">The content of the speech.</param>
        /// <param name="receiver">Optional. The receiver of the speech, if any.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        public IOperation DoCreatureSpeechAsync(uint creatureId, SpeechType speechType, ChatChannelType channelType, string content, string receiver = "")
        {
            var speechOp = new SpeechOperation(creatureId, speechType, channelType, content, receiver);

            this.DispatchOperation(speechOp);

            return speechOp;
        }

        /// <summary>
        /// Performs a creature turn asynchronously.
        /// </summary>
        /// <param name="requestorId">The id of the creature.</param>
        /// <param name="creature">The creature to turn.</param>
        /// <param name="direction">The direction to turn to.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        public IOperation DoCreatureTurnAsync(uint requestorId, ICreature creature, Direction direction)
        {
            var turnToDirOp = new TurnToDirectionOperation(creature, direction);

            this.DispatchOperation(turnToDirOp);

            return turnToDirOp;
        }

        /// <summary>
        /// Describes a thing for a player that is looking at it.
        /// </summary>
        /// <param name="thingId">The id of the thing to describe.</param>
        /// <param name="location">The location of the thing to describe.</param>
        /// <param name="stackPosition">The position in the stack within the location of the thing to describe.</param>
        /// <param name="player">The player for which to describe the thing for.</param>
        public void LookAt(ushort thingId, Location location, byte stackPosition, IPlayer player)
        {
            var lootAtOp = new LookAtOperation(thingId, location, stackPosition, player);

            this.DispatchOperation(lootAtOp);
        }

        /// <summary>
        /// Logs a player into the game.
        /// </summary>
        /// <param name="client">The client from which the player is connecting.</param>
        /// <param name="characterMetadata">The metadata for the player's creation.</param>
        public void LogPlayerIn(IClient client, CharacterEntity characterMetadata)
        {
            client.ThrowIfNull(nameof(client));
            characterMetadata.ThrowIfNull(nameof(characterMetadata));

            var loginOp = new LogInOperation(requestorId: 0, client, characterMetadata, this.WorldInfo.LightLevel, this.WorldInfo.LightColor);

            this.DispatchOperation(loginOp);
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

            var logOutOp = new LogOutOperation(player.Id, player);

            this.DispatchOperation(logOutOp);
        }

        /// <summary>
        /// Moves a thing.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the move.</param>
        /// <param name="clientThingId">The id of the thing being moved.</param>
        /// <param name="fromLocation">The location from which the thing is being moved.</param>
        /// <param name="fromIndex">The index within the location from which the thing is being moved.</param>
        /// <param name="fromCreatureId">The id of the creature from which the thing is being moved, if any.</param>
        /// <param name="toLocation">The location to which the thing is being moved.</param>
        /// <param name="toCreatureId">The id of the creature to which the thing is being moved.</param>
        /// <param name="amount">Optional. The amount of the thing to move. Defaults to 1.</param>
        public void Movement(uint requestorId, ushort clientThingId, Location fromLocation, byte fromIndex, uint fromCreatureId, Location toLocation, uint toCreatureId, byte amount = 1)
        {
            var scheduleDelay = TimeSpan.Zero;

            var creature = this.creatureManager.FindCreatureById(fromCreatureId);
            var movementOp = new MovementOperation(requestorId, clientThingId, fromLocation, fromIndex, fromCreatureId, toLocation, toCreatureId, amount);

            // Add delay from current exhaustion of the requestor if it's a creature, and this is a movement in the map.
            if (requestorId == fromCreatureId && fromLocation.Type == LocationType.Map && toLocation.Type == LocationType.Map)
            {
                // The scheduling delay becomes any cooldown debt for this operation.
                scheduleDelay = creature.RemainingExhaustionTime(ExhaustionFlag.Movement, this.scheduler.CurrentTime);
            }

            this.scheduler.ScheduleEvent(movementOp, scheduleDelay);
        }

        /// <summary>
        /// Attempts to add content to the first possible parent container that accepts it, on a chain of parent containers.
        /// </summary>
        /// <param name="thingContainer">The first thing container to add to.</param>
        /// <param name="remainder">The remainder content to add, which overflows to the next container in the chain.</param>
        /// <param name="addIndex">The index at which to attempt to add, only for the first attempted container.</param>
        /// <param name="includeTileAsFallback">Optional. A value for whether to include tiles in the fallback chain.</param>
        /// <param name="requestorCreature">Optional. The creature requesting the addition of content.</param>
        /// <returns>True if the content was successfully added, false otherwise.</returns>
        public bool AddContentToContainerOrFallback(IThingContainer thingContainer, ref IThing remainder, byte addIndex = byte.MaxValue, bool includeTileAsFallback = true, ICreature requestorCreature = null)
        {
            thingContainer.ThrowIfNull(nameof(thingContainer));

            const byte FallbackIndex = byte.MaxValue;

            bool success = false;
            bool firstAttempt = true;

            foreach (var targetContainer in thingContainer.GetParentContainerHierarchy(includeTileAsFallback))
            {
                IThing lastAddedThing = remainder;

                if (!success)
                {
                    (success, remainder) = targetContainer.AddContent(this.itemFactory, remainder, firstAttempt ? addIndex : FallbackIndex);
                }
                else if (remainder != null)
                {
                    (success, remainder) = targetContainer.AddContent(this.itemFactory, remainder);
                }

                firstAttempt = false;

                if (success)
                {
                    if (targetContainer is ITile targetTile)
                    {
                        var notification = new TileUpdatedNotification(
                                () => this.map.FindPlayersThatCanSee(targetTile.Location),
                                targetTile.Location,
                                this.mapDescriptor.DescribeTile);

                        this.scheduler.ScheduleEvent(notification);

                        // this.EventRulesApi.EvaluateRules(this, EventRuleType.Collision, new CollisionEventRuleArguments(targetContainer.Location, lastAddedThing, requestorCreature));
                    }

                    // this.EventRulesApi.EvaluateRules(this, EventRuleType.Movement, new MovementEventRuleArguments(lastAddedThing, requestorCreature));
                }

                if (success && remainder == null)
                {
                    break;
                }
            }

            return success;
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

            if (!this.map.GetTileAt(targetLocation, out ITile targetTile))
            {
                return false;
            }

            var (addSuccessful, _) = targetTile.AddContent(this.itemFactory, creature);

            if (addSuccessful)
            {
                this.creatureManager.RegisterCreature(creature);

                if (creature is ICombatant combatant)
                {
                    combatant.StatChanged += this.CreatureStatChanged;

                    combatant.Death += this.CombatantDeath;
                    combatant.AttackTargetChanged += this.CombatantAttackTargetChanged;
                    combatant.FollowTargetChanged += this.CreatureFollowTargetChanged;
                    combatant.LocationChanged += this.AfterThingLocationChanged;

                    // As a creature that can sense.
                    combatant.CreatureSeen += this.CreatureHasSeenCreature;
                    combatant.CreatureLost += this.CreatureHasLostCreature;

                    // As a skilled creature
                    combatant.SkillChanged += this.SkilledCreatureSkillChanged;

                    // Find now-spectators of this creature to start tracking that guy.
                    foreach (var spectator in this.map.FindCreaturesThatCanSee(creature.Location))
                    {
                        if (spectator is ICombatant combatantSpectator)
                        {
                            combatant.StartSensingCreature(combatantSpectator);
                            combatantSpectator.StartSensingCreature(combatant);
                        }
                    }
                }

                /*
                if (creature is IPlayer player)
                {
                    player.Inventory.SlotChanged += this.OnPlayerInventoryChanged;
                }
                */

                this.logger.LogDebug($"Placed {creature.Name} at {targetTile.Location}.");

                var placedAtStackPos = targetTile.GetStackOrderOfThing(creature);

                var notification = new CreatureMovedNotification(
                        () =>
                        {
                            if (creature is IPlayer player)
                            {
                                return this.map.FindPlayersThatCanSee(creature.Location).Except(player.YieldSingleItem());
                            }

                            return this.map.FindPlayersThatCanSee(creature.Location);
                        },
                        creature.Id,
                        default,
                        byte.MaxValue,
                        creature.Location,
                        placedAtStackPos,
                        wasTeleport: true);

                this.scheduler.ScheduleEvent(notification);
            }

            return addSuccessful;
        }

        /// <summary>
        /// Places a new monster of the given race, at the given location.
        /// </summary>
        /// <param name="raceId">The id of race of monster to place.</param>
        /// <param name="location">The location at which to place the monster.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        public IOperation PlaceNewMonsterAtAsync(string raceId, Location location)
        {
            using var uow = this.applicationContext.CreateNewUnitOfWork();

            var monsterType = uow.MonsterTypes.GetByRaceId(raceId);

            if (monsterType == null)
            {
                this.logger.LogWarning($"Unable to place monster. Could not find a monster with the id {raceId} in the repository. ({nameof(this.PlaceNewMonsterAtAsync)})");

                return null;
            }

            var newMonster = this.creatureFactory.Create(new CreatureCreationArguments() { Type = CreatureType.Monster, Metadata = monsterType }) as IMonster;

            if (!this.map.GetTileAt(location, out ITile targetTile) || targetTile.IsPathBlocking())
            {
                return null;
            }

            var placeMonsterOp = new PlaceCreatureOperation(requestorId: 0, targetTile, newMonster);

            this.DispatchOperation(placeMonsterOp);

            return placeMonsterOp;
        }

        /// <summary>
        /// Attempts to remove a creature from the game.
        /// </summary>
        /// <param name="creature">The creature to remove.</param>
        /// <returns>True if the creature is successfully removed from the game, false otherwise.</returns>
        public bool RemoveCreatureFromGame(ICreature creature)
        {
            if (!this.map.GetTileAt(creature.Location, out ITile fromTile))
            {
                return false;
            }

            var oldStackpos = fromTile.GetStackOrderOfThing(creature);

            IThing creatureAsThing = creature;

            var removedFromTile = fromTile.RemoveContent(this.itemFactory, ref creatureAsThing).result;

            if (removedFromTile)
            {
                if (creature is ICombatant combatant)
                {
                    combatant.SetAttackTarget(null);

                    foreach (var attacker in combatant.AttackedBy)
                    {
                        attacker.StopSensingCreature(combatant);
                    }

                    foreach (var trackedCreature in combatant.TrackedCreatures)
                    {
                        combatant.StopSensingCreature(trackedCreature);

                        if (trackedCreature is ICreatureThatSensesOthers creatureThatSensesOthers)
                        {
                            creatureThatSensesOthers.StopSensingCreature(combatant);
                        }
                    }

                    combatant.StatChanged -= this.CreatureStatChanged;

                    combatant.Death -= this.CombatantDeath;
                    combatant.AttackTargetChanged -= this.CombatantAttackTargetChanged;
                    combatant.FollowTargetChanged -= this.CreatureFollowTargetChanged;
                    combatant.LocationChanged -= this.AfterThingLocationChanged;

                    // As a creature that can sense.
                    combatant.CreatureSeen -= this.CreatureHasSeenCreature;
                    combatant.CreatureLost -= this.CreatureHasLostCreature;

                    // As a skilled creature
                    combatant.SkillChanged -= this.SkilledCreatureSkillChanged;
                }

                if (creature is IPlayer player)
                {
                    var notification = new CreatureRemovedNotification(
                            () => this.map.FindPlayersThatCanSee(creature.Location).Union(player.YieldSingleItem()),
                            creature,
                            oldStackpos);

                    this.scheduler.ScheduleEvent(notification);

                    // player.Inventory.SlotChanged -= context.GameApi.OnPlayerInventoryChanged;
                }
                else
                {
                    var notification = new CreatureRemovedNotification(() => this.map.FindPlayersThatCanSee(creature.Location), creature, oldStackpos);

                    this.scheduler.ScheduleEvent(notification);
                }
            }

            return removedFromTile;
        }

        /// <summary>
        /// Sends a heartbeat to the player's client.
        /// </summary>
        /// <param name="player">The player which to send the heartbeat to.</param>
        /// <returns>The notification that was scheduled, or null if nothing happened.</returns>
        public INotification SendHeartbeatAsync(IPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            var notification = new GenericNotification(() => player.YieldSingleItem(), new HeartbeatPacket());

            this.scheduler.ScheduleEvent(notification);

            return notification;
        }

        /// <summary>
        /// Sends a heartbeat response to the player's client.
        /// </summary>
        /// <param name="player">The player which to send the heartbeat response to.</param>
        /// <returns>The notification that was scheduled, or null if nothing happened.</returns>
        public INotification SendHeartbeatResponseAsync(IPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            var notification = new GenericNotification(() => player.YieldSingleItem(), new HeartbeatResponsePacket());

            this.scheduler.ScheduleEvent(notification);

            return notification;
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

                if (combatant is IPlayer attackerPlayer)
                {
                    this.AddOrAggregateCondition(
                        attackerPlayer,
                        new InFightCondition(attackerPlayer),
                        duration: TimeSpan.FromMilliseconds(CombatConstants.DefaultInFightTimeInMs));
                }

                if (combatant.AutoAttackTarget is IPlayer targetPlayer)
                {
                    this.AddOrAggregateCondition(
                        targetPlayer,
                        new InFightCondition(targetPlayer),
                        duration: TimeSpan.FromMilliseconds(CombatConstants.DefaultInFightTimeInMs));
                }
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
                this.ResetCreatureWalkPlan(combatant, combatant.ChaseTarget, targetDistance: combatant.AutoAttackRange);
            }
        }

        /// <summary>
        /// Handles the event when a creature has seen another creature.
        /// </summary>
        /// <param name="creature">The monster that sees the other.</param>
        /// <param name="creatureSeen">The creature that was seen.</param>
        private void CreatureHasSeenCreature(ICreatureThatSensesOthers creature, ICreature creatureSeen)
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
        private void CreatureHasLostCreature(ICreatureThatSensesOthers creature, ICreature creatureLost)
        {
            creature.ThrowIfNull(nameof(creature));
            creatureLost.ThrowIfNull(nameof(creatureLost));

            if (creature is ICombatant combatant && creatureLost is ICombatant combatantLost)
            {
                combatant.RemoveFromCombatList(combatantLost);
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
        private void SkilledCreatureSkillChanged(ICreatureWithSkills skilledCreature, ISkill skillThatChanged, uint previousLevel, byte previousPercent, long? countDelta = null)
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
                    this.scheduler.ScheduleEvent(new TextMessageNotification(() => player.YieldSingleItem(), MessageType.EventAdvance, skillChangeNotificationMessage));
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
                this.scheduler.ScheduleEvent(new GenericNotification(() => player.YieldSingleItem(), new PlayerStatsPacket(player), new PlayerSkillsPacket(player)));

                // Add the animated text for experience.
                if (skillThatChanged.Type == SkillType.Experience && countDelta.HasValue && countDelta.Value > 0)
                {
                    this.scheduler.ScheduleEvent(new AnimatedTextNotification(() => this.map.FindPlayersThatCanSee(player.Location), player.Location, TextColor.White, countDelta.Value.ToString()));
                }
            }
        }

        /// <summary>
        /// Handles the aftermath of a location change from a thing.
        /// </summary>
        /// <param name="thing">The thing which's location changed.</param>
        /// <param name="previousLocation">The previous location of the thing.</param>
        private void AfterThingLocationChanged(IThing thing, Location previousLocation)
        {
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
                    spectator.StartSensingCreature(movingCombatant);
                    movingCombatant.StartSensingCreature(spectator);
                }

                // Now make old spectators aware that this creature moved out of their view.
                foreach (var spectator in spectatorsLost)
                {
                    spectator.StopSensingCreature(movingCombatant);
                    movingCombatant.StopSensingCreature(spectator);
                }
            }

            /*
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
        private void CreatureStatChanged(ICreature creature, IStat statThatChanged, uint previousValue, byte previousPercent)
        {
            creature.ThrowIfNull(nameof(creature));
            statThatChanged.ThrowIfNull(nameof(statThatChanged));

            if (statThatChanged.Type == CreatureStat.HitPoints)
            {
                // This updates the health as seen by others.
                this.scheduler.ScheduleEvent(new GenericNotification(() => this.map.FindPlayersThatCanSee(creature.Location), new CreatureHealthPacket(creature)));
            }

            if (creature is IPlayer player)
            {
                if (statThatChanged.Type == CreatureStat.BaseSpeed)
                {
                    this.scheduler.ScheduleEvent(new GenericNotification(() => player.YieldSingleItem(), new CreatureSpeedChangePacket(player)));
                }

                // And this updates the health of the player.
                this.scheduler.ScheduleEvent(new GenericNotification(() => player.YieldSingleItem(), new PlayerStatsPacket(player)));
            }
        }

        /// <summary>
        /// Handles the death of a combatant.
        /// </summary>
        /// <param name="combatant">The combatant that died.</param>
        private void CombatantDeath(ICombatant combatant)
        {
            if (combatant is IPlayer playerCombatant)
            {
                this.CancelPlayerOperationsAsync(playerCombatant, typeof(IOperation));
            }

            var rng = new Random();
            var deathDelay = TimeSpan.FromMilliseconds(rng.Next(MechanicsConstants.DefaultDeathDelayMs));
            var deathOp = new DeathOperation(requestorId: 0, combatant);

            this.DispatchOperation(deathOp, deathDelay);
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
        /// Cleans up players who's connections are orphaned, or (TODO) have been idle for some time.
        /// </summary>
        /// <param name="tokenState">The state object which gets casted into a <see cref="CancellationToken"/>.</param>.
        private void IdlePlayerSweep(object tokenState)
        {
            var cancellationToken = (tokenState as CancellationToken?).Value;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Thread.Sleep is OK here because this runs on it's own thread.
                Thread.Sleep(IdleCheckDelay);

                // Now let's clean up and try to log out all orphaned ones.
                foreach (var player in this.creatureManager.FindAllPlayers())
                {
                    if (player.Client != null && !player.Client.IsIdle)
                    {
                        if (player.Client.Information.Type == AgentType.Linux ||
                            player.Client.Information.Type == AgentType.OtClientLinux ||
                            player.Client.Information.Type == AgentType.OtClientWindows)
                        {
                            this.SendHeartbeatAsync(player);
                        }

                        continue;
                    }

                    var logOutOp = new LogOutOperation(player.Id, player);

                    this.DispatchOperation(logOutOp);

                    if (player is ICombatant playerAsCombatant)
                    {
                        playerAsCombatant.SetAttackTarget(null);
                    }
                }
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

                // A day is roughly an hour in real time, and night lasts roughly 1/3 of the day in real time
                // Dusk and Dawns last for 30 minutes roughly, so les aproximate that to 2 minutes.
                var currentMinute = this.scheduler.CurrentTime.Minute;

                var currentLevel = this.worldInfo.LightLevel;
                var currentColor = this.worldInfo.LightColor;

                if (currentMinute >= 0 && currentMinute <= 37)
                {
                    // Day time: [0, 37] minutes on the hour.
                    this.worldInfo.LightLevel = LightConstants.WorldDay;
                    this.worldInfo.LightColor = LightConstants.WhiteColor;
                }
                else if (currentMinute == 38 || currentMinute == 39 || currentMinute == 58 || currentMinute == 59)
                {
                    // Dusk: [38, 39] minutes on the hour.
                    // Dawn: [58, 59] minutes on the hour.
                    this.worldInfo.LightLevel = LightConstants.WorldDuskDawn;
                    this.worldInfo.LightColor = LightConstants.OrangeColor;
                }
                else if (currentMinute >= 40 && currentMinute <= 57)
                {
                    // Night time: [40, 57] minutes on the hour.
                    this.worldInfo.LightLevel = LightConstants.WorldNight;
                    this.worldInfo.LightColor = LightConstants.WhiteColor;
                }

                if (this.worldInfo.LightLevel != currentLevel || this.worldInfo.LightColor != currentColor)
                {
                    this.scheduler.ScheduleEvent(
                        new WorldLightChangedNotification(
                            () => this.creatureManager.FindAllPlayers(),
                            this.worldInfo.LightLevel,
                            this.worldInfo.LightColor));
                }

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
        /// Handles a signal from the scheduler that an event has been fired and begins processing it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The arguments of the event.</param>
        private void ProcessFiredEvent(object sender, EventFiredEventArgs eventArgs)
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

                this.logger.LogTrace($"Processed {evt.GetType().Name} with id: {evt.EventId}, current game time: {this.scheduler.CurrentTime.ToUnixTimeMilliseconds()}.");
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

            if (typeof(IElevatedOperation).IsAssignableFrom(eventType))
            {
                return new ElevatedOperationContext(
                    this.logger,
                    this.applicationContext,
                    this.mapDescriptor,
                    this.map,
                    this.creatureManager,
                    this.itemFactory,
                    this.creatureFactory,
                    this.containerManager,
                    this,
                    this,
                    this.pathFinder,
                    this.predefinedItemSet,
                    this.scheduler);
            }
            else if (typeof(IOperation).IsAssignableFrom(eventType))
            {
                return new OperationContext(
                    this.logger,
                    this.mapDescriptor,
                    this.map,
                    this.creatureManager,
                    this.itemFactory,
                    this.creatureFactory,
                    this.containerManager,
                    this,
                    this,
                    this.pathFinder,
                    this.predefinedItemSet,
                    this.scheduler);
            }
            else if (typeof(ICondition).IsAssignableFrom(eventType))
            {
                return new ConditionContext(
                    this.logger,
                    this.mapDescriptor,
                    this.map,
                    this.creatureManager,
                    this.itemFactory,
                    this.scheduler);
            }
            else if (typeof(INotification).IsAssignableFrom(eventType))
            {
                return new NotificationContext(
                    this.logger,
                    this.mapDescriptor,
                    this.creatureManager);
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
        /// Handles commonly executed logic on any item being created.
        /// </summary>
        /// <param name="itemCreated">The item that was created.</param>
        private void AfterItemIsCreated(IItem itemCreated)
        {
            itemCreated.ThrowIfNull(nameof(itemCreated));

            this.logger.LogTrace($"Item created: {itemCreated.DescribeForLogger()}.");

            // Start decay for items that need it.
            if (itemCreated.HasExpiration)
            {
                this.AddOrAggregateCondition(itemCreated, new DecayingCondition(itemCreated), itemCreated.ExpirationTimeLeft);
            }
        }
    }
}
