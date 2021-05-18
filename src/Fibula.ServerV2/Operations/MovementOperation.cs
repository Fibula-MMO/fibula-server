// -----------------------------------------------------------------
// <copyright file="MovementOperation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Operations
{
    using System;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Definitions.Flags;
    using Fibula.Plugins.PathFinding.AStar.Extensions;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Constants;
    using Fibula.ServerV2.Contracts.Extensions;
    using Fibula.ServerV2.Creatures;
    using Fibula.ServerV2.Extensions;
    using Fibula.ServerV2.Notifications;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a common base between movements.
    /// </summary>
    public class MovementOperation : Operation
    {
        /// <summary>
        /// The default exhaustion cost for movements.
        /// </summary>
        private static readonly TimeSpan DefaultMovementExhaustionCost = TimeSpan.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the movement.</param>
        /// <param name="thingId">The id of the thing that is moving.</param>
        /// <param name="fromLocation">The location from which the movement is happening.</param>
        /// <param name="fromIndex">The index within the location from which the movement is happening.</param>
        /// <param name="fromCreatureId">The id of the creature from which the movement is happening, if any.</param>
        /// <param name="toLocation">The location to which the movement is happening.</param>
        /// <param name="toCreatureId">The id of the creature to which the movement is happening, if any.</param>
        /// <param name="amount">The amount of the thing to move.</param>
        public MovementOperation(
            uint requestorId,
            ushort thingId,
            Location fromLocation,
            byte fromIndex,
            uint fromCreatureId,
            Location toLocation,
            uint toCreatureId,
            byte amount)
            : base(requestorId)
        {
            this.ThingMovingId = thingId;
            this.FromLocation = fromLocation;
            this.FromIndex = fromIndex;
            this.FromCreatureId = fromCreatureId;
            this.ToLocation = toLocation;
            this.ToCreatureId = toCreatureId;
            this.Amount = amount;

            this.ExhaustionInfo.Add(ExhaustionFlag.Movement, TimeSpan.Zero);
        }

        /// <summary>
        /// Gets the id of the thing moving.
        /// </summary>
        public ushort ThingMovingId { get; }

        /// <summary>
        /// Gets the location from which the movement happens.
        /// </summary>
        public Location FromLocation { get; }

        /// <summary>
        /// Gets the index within the location from which the movement is happening.
        /// </summary>
        public byte FromIndex { get; }

        /// <summary>
        /// Gets the id of the creature carrying the thing that is being moved, if any.
        /// </summary>
        public uint FromCreatureId { get; }

        /// <summary>
        /// Gets the location to which the movement happens.
        /// </summary>
        public Location ToLocation { get; }

        /// <summary>
        /// Gets the id of the creature that will carry the thing that is being moved, if any.
        /// </summary>
        public uint ToCreatureId { get; }

        /// <summary>
        /// Gets the amount of thing being moved.
        /// </summary>
        public byte Amount { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            switch (this.FromLocation.Type)
            {
                case LocationType.Map:
                    var fromTile = context.Map[this.FromLocation];

                    if (fromTile == null)
                    {
                        this.DispatchTextNotification(context);

                        return;
                    }

                    // Request the actual movement.
                    switch (this.ToLocation.Type)
                    {
                        case LocationType.Map:
                            this.MapToMapMovement(context, fromTile, context.Map[this.ToLocation], isTeleport: false);

                            break;
                        case LocationType.InsideContainer:
                            this.MapToContainer(context, fromTile, context.ContainerManager.FindForCreature(this.ToCreatureId, this.ToLocation.ContainerId));

                            break;
                        case LocationType.InventorySlot:
                            var toCreature = context.CreatureManager.FindCreatureById(this.ToCreatureId);

                            this.MapToBody(context, fromTile, toCreature?[(byte)this.ToLocation.Slot] as IContainerItem);

                            break;
                    }

                    break;
                case LocationType.InsideContainer:
                    var sourceContainer = context.ContainerManager.FindForCreature(this.FromCreatureId, this.FromLocation.ContainerId);

                    // We know the source location is good, now let's figure out the destination to create the appropriate movement event.
                    switch (this.ToLocation.Type)
                    {
                        case LocationType.Map:
                            this.ContainerToMap(context, sourceContainer, context.Map[this.ToLocation]);

                            break;
                        case LocationType.InsideContainer:
                            this.ContainerToContainer(context, sourceContainer, context.ContainerManager.FindForCreature(this.ToCreatureId, this.ToLocation.ContainerId));

                            break;
                        case LocationType.InventorySlot:
                            var toCreature = context.CreatureManager.FindCreatureById(this.ToCreatureId);

                            this.ContainerToBody(context, sourceContainer, toCreature?[(byte)this.ToLocation.Slot] as IContainerItem);

                            break;
                    }

                    break;
                case LocationType.InventorySlot:
                    var fromCreature = context.CreatureManager.FindCreatureById(this.FromCreatureId);

                    if (fromCreature == null)
                    {
                        this.DispatchTextNotification(context, OperationMessage.MustFirstOpenThatContainer);

                        return;
                    }

                    var sourceBodyContainer = fromCreature[(byte)this.FromLocation.Slot] as IContainerItem;

                    // We know the source location is good, now let's figure out the destination to create the appropriate movement event.
                    switch (this.ToLocation.Type)
                    {
                        case LocationType.Map:
                            this.BodyToMap(context, sourceBodyContainer, context.Map[this.ToLocation]);

                            break;
                        case LocationType.InsideContainer:
                            // this.BodyToContainer(context, sourceBodyContainer, context.ContainerManager.FindForCreature(this.ToCreatureId, this.ToLocation.ContainerId));
                            break;
                        case LocationType.InventorySlot:
                            var toCreature = context.CreatureManager.FindCreatureById(this.ToCreatureId);

                            // this.BodyToBody(context, sourceBodyContainer, toCreature?.Inventory[(byte)this.ToLocation.Slot] as IContainerItem);
                            break;
                    }

                    break;
            }
        }

        private void BodyToBody(IOperationContext context, IContainerItem sourceContainer, IContainerItem destinationContainer)
        {
            var item = sourceContainer?[this.FromLocation.ContainerIndex];

            if (item == null)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!this.PerformItemMovement(context, item, sourceContainer, destinationContainer, 0, 0, this.Amount, this.GetRequestor(context.CreatureManager)))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
        }

        private void BodyToContainer(IOperationContext context, IContainerItem sourceContainer, IContainerItem destinationContainer)
        {
            // Declare some pre-conditions.
            var creatureHasDestinationContainerOpen = destinationContainer != null;

            var item = sourceContainer?[this.FromLocation.ContainerIndex];

            if (item == null)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!creatureHasDestinationContainerOpen)
            {
                this.DispatchTextNotification(context, OperationMessage.MustFirstOpenThatContainer);
            }
            else if (!this.PerformItemMovement(context, item, sourceContainer, destinationContainer, 0, this.ToLocation.ContainerIndex, this.Amount, this.GetRequestor(context.CreatureManager)))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
        }

        private void BodyToMap(IOperationContext context, IContainerItem sourceContainer, ITile destinationTile)
        {
            var item = sourceContainer?[this.FromLocation.ContainerIndex];

            if (item == null)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);

                return;
            }

            // Declare some pre-conditions.
            var destinationHasGround = destinationTile?.Ground != null;
            var destinationIsObstructed = destinationTile.BlocksLay || (item.BlocksPass && destinationTile.BlocksPass);
            var canThrowBetweenLocations = context.Map.CanThrowBetweenLocations(sourceContainer.Location, this.ToLocation, checkLineOfSight: true);

            if (!destinationHasGround || !canThrowBetweenLocations)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotThrowThere);
            }
            else if (destinationIsObstructed)
            {
                this.DispatchTextNotification(context, OperationMessage.NotEnoughRoom);
            }
            else if (!this.PerformItemMovement(context, item, sourceContainer, destinationTile, 0, amountToMove: this.Amount, requestorCreature: this.GetRequestor(context.CreatureManager)))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
            else if (this.GetRequestor(context.CreatureManager) is IPlayer player && this.ToLocation != player.Location && player != item)
            {
                var directionToDestination = player.Location.DirectionTo(this.ToLocation);

                context.Scheduler.ScheduleEvent(new TurnToDirectionOperation(player, directionToDestination));
            }
        }

        private void ContainerToBody(IOperationContext context, IContainerItem sourceContainer, IContainerItem destinationContainer)
        {
            // Declare some pre-conditions.
            var creatureHasSourceContainerOpen = destinationContainer != null;

            var item = sourceContainer?[this.FromLocation.ContainerIndex];

            if (item == null)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!creatureHasSourceContainerOpen)
            {
                this.DispatchTextNotification(context, OperationMessage.MustFirstOpenThatContainer);
            }
            else if (!this.PerformItemMovement(context, item, sourceContainer, destinationContainer, this.FromLocation.ContainerIndex, 0, this.Amount, this.GetRequestor(context.CreatureManager)))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
        }

        private void ContainerToContainer(IOperationContext context, IContainerItem sourceContainer, IContainerItem destinationContainer)
        {
            // Declare some pre-conditions.
            var creatureHasSourceContainerOpen = sourceContainer != null;
            var creatureHasDestinationContainerOpen = destinationContainer != null;

            var item = sourceContainer?[this.FromLocation.ContainerIndex];

            if (item == null)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!creatureHasSourceContainerOpen)
            {
                this.DispatchTextNotification(context, OperationMessage.MustFirstOpenThatContainer);
            }
            else if (!creatureHasDestinationContainerOpen)
            {
                this.DispatchTextNotification(context, OperationMessage.MustFirstOpenThatContainer);
            }
            else if (!this.PerformItemMovement(context, item, sourceContainer, destinationContainer, this.FromLocation.ContainerIndex, this.ToLocation.ContainerIndex, this.Amount, this.GetRequestor(context.CreatureManager)))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
        }

        private void ContainerToMap(IOperationContext context, IContainerItem sourceContainer, ITile destinationTile)
        {
            var item = sourceContainer?[this.FromLocation.ContainerIndex];

            if (item == null)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);

                return;
            }

            // Declare some pre-conditions.
            var destinationHasGround = destinationTile?.Ground != null;
            var destinationIsObstructed = destinationTile.BlocksLay || (item.BlocksPass && destinationTile.BlocksPass);
            var creatureHasSourceContainerOpen = sourceContainer != null;
            var canThrowBetweenLocations = context.Map.CanThrowBetweenLocations(sourceContainer.Location, this.ToLocation, checkLineOfSight: true);

            if (!destinationHasGround || !canThrowBetweenLocations)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotThrowThere);
            }
            else if (destinationIsObstructed)
            {
                this.DispatchTextNotification(context, OperationMessage.NotEnoughRoom);
            }
            else if (!creatureHasSourceContainerOpen)
            {
                this.DispatchTextNotification(context, OperationMessage.MustFirstOpenThatContainer);
            }
            else if (!this.PerformItemMovement(context, item, sourceContainer, destinationTile, fromIndex: this.FromLocation.ContainerIndex, amountToMove: this.Amount, requestorCreature: this.GetRequestor(context.CreatureManager)))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
            else if (this.GetRequestor(context.CreatureManager) is IPlayer player && this.ToLocation != player.Location && player != item)
            {
                var directionToDestination = player.Location.DirectionTo(this.ToLocation);

                context.Scheduler.ScheduleEvent(new TurnToDirectionOperation(player, directionToDestination));
            }
        }

        private void MapToBody(IOperationContext context, ITile sourceTile, IContainerItem destinationContainer)
        {
            var requestor = this.GetRequestor(context.CreatureManager);
            var itemMoving = sourceTile.TopItem;

            // Declare some pre-conditions.
            var sourceTileIsNull = sourceTile == null;
            var thingCanBeMoved = itemMoving != null && (itemMoving == requestor || itemMoving.CanBeMoved);
            var locationsMatch = itemMoving?.Location == this.FromLocation;
            var requestorInRange = requestor == null || (requestor.Location - this.FromLocation).MaxValueIn2D <= 1;
            var sourceTileHasEnoughItemAmount = this.ThingMovingId == itemMoving.TypeId && itemMoving.Amount >= this.Amount;

            if (sourceTileIsNull || !thingCanBeMoved)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!locationsMatch)
            {
                // Silent fail.
                return;
            }
            else if (!sourceTileHasEnoughItemAmount)
            {
                this.DispatchTextNotification(context, OperationMessage.NotEnoughQuantity);
            }
            else if (!requestorInRange)
            {
                this.DispatchTextNotification(context, OperationMessage.TooFarAway);
            }
            else if (!this.PerformItemMovement(context, itemMoving, sourceTile, destinationContainer, toIndex: 0, amountToMove: this.Amount, requestorCreature: requestor))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
        }

        private void MapToContainer(IOperationContext context, ITile sourceTile, IContainerItem destinationContainer)
        {
            var requestor = this.GetRequestor(context.CreatureManager);
            var itemMoving = sourceTile.TopItem;

            // Declare some pre-conditions.
            var sourceTileIsNull = sourceTile == null;
            var thingCanBeMoved = itemMoving != null && itemMoving.CanBeMoved;
            var locationsMatch = itemMoving?.Location == this.FromLocation;
            var requestorInRange = requestor == null || (requestor.Location - this.FromLocation).MaxValueIn2D <= 1;
            var creatureHasDestinationContainerOpen = destinationContainer != null;
            var sourceTileHasEnoughItemAmount = this.ThingMovingId == itemMoving.TypeId && itemMoving.Amount >= this.Amount;

            if (sourceTileIsNull || !thingCanBeMoved)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!locationsMatch)
            {
                // Silent fail.
                return;
            }
            else if (!sourceTileHasEnoughItemAmount)
            {
                this.DispatchTextNotification(context, OperationMessage.NotEnoughQuantity);
            }
            else if (!creatureHasDestinationContainerOpen)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotThrowThere);
            }
            else if (!requestorInRange)
            {
                this.DispatchTextNotification(context, OperationMessage.TooFarAway);
            }
            else if (!this.PerformItemMovement(context, itemMoving, sourceTile, destinationContainer, toIndex: this.ToLocation.ContainerIndex, amountToMove: this.Amount, requestorCreature: requestor))
            {
                // Something else went wrong.
                this.DispatchTextNotification(context);
            }
        }

        private void MapToMapMovement(IOperationContext context, ITile sourceTile, ITile destinationTile, bool isTeleport)
        {
            var requestor = this.GetRequestor(context.CreatureManager);

            IThing thingMoving = this.ThingMovingId == CreatureConstants.CreatureTypeId ? sourceTile.TopCreature : sourceTile.TopItem;

            // Declare some pre-conditions.
            var sourceTileIsNull = sourceTile == null;
            var destinationHasGround = destinationTile?.Ground != null;
            var thingExists = thingMoving != null;
            var locationsMatch = thingMoving?.Location == this.FromLocation;
            var isIntendedThing = this.ThingMovingId != CreatureConstants.CreatureTypeId ? thingMoving?.TypeId == this.ThingMovingId : (thingMoving as ICreature)?.Id == this.FromCreatureId;
            var requestorInRange = requestor == null || (requestor.Location - this.FromLocation).MaxValueIn2D <= 1;
            var canThrowBetweenLocations = isTeleport || requestor == null || context.Map.CanThrowBetweenLocations(this.FromLocation, this.ToLocation, checkLineOfSight: true);

            if (sourceTileIsNull || !locationsMatch || !isIntendedThing)
            {
                // Silent fail.
                return;
            }
            else if (!thingExists)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
            }
            else if (!destinationHasGround || !canThrowBetweenLocations)
            {
                this.DispatchTextNotification(context, OperationMessage.MayNotThrowThere);
            }
            else if (thingMoving is ICreature creature)
            {
                var distanceBetweenLocations = this.FromLocation - this.ToLocation;

                // More pre-conditions.
                var creatureCanBeMoved = (thingMoving == requestor && requestor.CanWalk) || requestor.CanPush(creature);
                var canThrowThatFar = isTeleport || requestor == null || (distanceBetweenLocations.MaxValueIn2D <= 1 && distanceBetweenLocations.Z == 0);
                var creatureAvoidsDestination = !isTeleport && requestor != null && requestor != creature && destinationTile.IsPathBlocking(/*this.Requestor.DamageTypesToAvoid*/);
                var destinationIsObstructed = !isTeleport && distanceBetweenLocations.Z == 0 && (destinationTile.BlocksLay || destinationTile.BlocksPass);

                if (creatureAvoidsDestination)
                {
                    this.DispatchTextNotification(context, OperationMessage.NotEnoughRoom);
                }
                else if (destinationIsObstructed)
                {
                    if (requestor != null && creature.Id == this.RequestorId && creature is IPlayer player)
                    {
                        this.SendNotification(context, new PlayerCancelWalkNotification(player));
                    }

                    this.DispatchTextNotification(context, OperationMessage.NotEnoughRoom);
                }
                else if (!requestorInRange)
                {
                    this.DispatchTextNotification(context, OperationMessage.TooFarAway);
                }
                else if (!canThrowThatFar)
                {
                    this.DispatchTextNotification(context, OperationMessage.DestinationTooFarAway);
                }
                else if (!this.PerformCreatureMovement(context, creature, this.ToLocation, isTeleport, requestorCreature: requestor))
                {
                    // Something else went wrong.
                    this.DispatchTextNotification(context);
                }
                else if (requestor is IPlayer player && this.ToLocation != player.Location && player != thingMoving)
                {
                    var directionToDestination = player.Location.DirectionTo(this.ToLocation);

                    context.Scheduler.ScheduleEvent(new TurnToDirectionOperation(player, directionToDestination));
                }
            }
            else if (thingMoving is IItem item)
            {
                var distanceBetweenLocations = (requestor?.Location ?? this.FromLocation) - this.ToLocation;
                var distanceFromSource = (requestor?.Location ?? this.FromLocation) - this.FromLocation;

                // More pre-conditions.
                var itemCanBeMoved = item.CanBeMoved;
                var sourceTileHasEnoughItemAmount = this.ThingMovingId == item.TypeId && item.Amount >= this.Amount;
                var destinationIsObstructed = destinationTile.BlocksLay || (item.BlocksPass && destinationTile.BlocksPass);
                var movementInRange = requestor == null || (distanceFromSource.MaxValueIn2D <= 1 && distanceFromSource.Z == 0 && (!item.Type.HasItemFlag(ItemFlag.BlocksWalk) || (distanceBetweenLocations.MaxValueIn2D <= 2 && distanceBetweenLocations.Z == 0)));

                if (!itemCanBeMoved)
                {
                    this.DispatchTextNotification(context, OperationMessage.MayNotMoveThis);
                }
                else if (destinationIsObstructed)
                {
                    this.DispatchTextNotification(context, OperationMessage.MayNotThrowThere);
                }
                else if (!movementInRange)
                {
                    this.DispatchTextNotification(context, OperationMessage.DestinationTooFarAway);
                }
                else if (!sourceTileHasEnoughItemAmount)
                {
                    this.DispatchTextNotification(context, OperationMessage.NotEnoughQuantity);
                }
                else if (!this.PerformItemMovement(context, item, sourceTile, destinationTile, this.FromIndex, amountToMove: this.Amount, requestorCreature: requestor))
                {
                    // Something else went wrong.
                    this.DispatchTextNotification(context);
                }
                else if (requestor is IPlayer player && this.ToLocation != player.Location && player != thingMoving)
                {
                    var directionToDestination = player.Location.DirectionTo(this.ToLocation);

                    context.Scheduler.ScheduleEvent(new TurnToDirectionOperation(player, directionToDestination));
                }

                // TODO: Check if the item is an IContainerItem and, if it got moved, check if there are players that have it open that now are too far away from it.
            }
        }

        /// <summary>
        /// Immediately attempts to perform an item movement between two containers.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        /// <param name="item">The item being moved.</param>
        /// <param name="fromContainer">The container from which the movement is being performed.</param>
        /// <param name="toContainer">The container to which the movement is being performed.</param>
        /// <param name="fromIndex">Optional. The index within the container to move the item from.</param>
        /// <param name="toIndex">Optional. The index within the container to move the item to.</param>
        /// <param name="amountToMove">Optional. The amount of the thing to move. Defaults to 1.</param>
        /// <param name="requestorCreature">Optional. The creature that this movement is being performed in behalf of, if any.</param>
        /// <returns>True if the movement was successfully performed, false otherwise.</returns>
        /// <remarks>Changes game state, should only be performed after all pertinent validations happen.</remarks>
        private bool PerformItemMovement(IOperationContext context, IItem item, IItemsContainer fromContainer, IItemsContainer toContainer, byte fromIndex = byte.MaxValue, byte toIndex = byte.MaxValue, byte amountToMove = 1, ICreature requestorCreature = null)
        {
            const byte FallbackIndex = byte.MaxValue;

            if (item == null || fromContainer == null || toContainer == null)
            {
                return false;
            }

            var sameContainer = fromContainer == toContainer;

            if (sameContainer && fromIndex == toIndex)
            {
                // no change at all.
                return true;
            }

            // Edge case, check if the moving item is the target container.
            if (item is IContainerItem containerItem && toContainer is IContainerItem targetContainer && targetContainer.IsDescendantOf(containerItem))
            {
                return false;
            }

            (bool removeSuccessful, IThing removeRemainder) = fromContainer.RemoveItem(context.ItemFactory, ref item, amountToMove, fromIndex);

            if (!removeSuccessful)
            {
                // Failing to remove the item from the original container stops the entire operation.
                return false;
            }

            if (fromContainer is ITile fromTile)
            {
                this.SendNotification(context, new TileUpdatedNotification(context.Map.FindPlayersThatCanSee(fromTile.Location), fromTile));
            }

            /* context.EventRulesApi.EvaluateRules(this, EventRuleType.Separation, new SeparationEventRuleArguments(fromThingContainer.Location, item, requestorCreature)); */

            IItem remainder = item;

            if (sameContainer && removeRemainder == null && fromIndex < toIndex)
            {
                // If the move happens within the same container, we need to adjust the index of where we're adding, depending if it is before or after.
                toIndex--;
            }

            if (!toContainer.AddItemToContainerRecursively(context.ItemFactory, ref remainder, toIndex, includeTileAsFallback: false) || remainder != null)
            {
                // There is some rollback to do, as we failed to add the entire thing.
                IItem rollbackRemainder = remainder ?? item;

                if (!fromContainer.AddItemToContainerRecursively(context.ItemFactory, ref rollbackRemainder, FallbackIndex, includeTileAsFallback: true))
                {
                    context.Logger.LogError($"Rollback failed on {nameof(this.PerformItemMovement)}. Thing: {rollbackRemainder.DescribeForLogger()}");
                }
            }

            return true;
        }

        /// <summary>
        /// Immediately attempts to perform a creature movement to a tile on the map.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        /// <param name="creature">The creature being moved.</param>
        /// <param name="toLocation">The tile to which the movement is being performed.</param>
        /// <param name="isTeleport">Optional. A value indicating whether the movement is considered a teleportation. Defaults to false.</param>
        /// <param name="requestorCreature">Optional. The creature that this movement is being performed in behalf of, if any.</param>
        /// <returns>True if the movement was successfully performed, false otherwise.</returns>
        /// <remarks>Changes game state, should only be performed after all pertinent validations happen.</remarks>
        private bool PerformCreatureMovement(IOperationContext context, ICreature creature, Location toLocation, bool isTeleport = false, ICreature requestorCreature = null)
        {
            if (creature == null || !(creature.ParentContainer is ITile fromTile) || !context.Map.HasTileAt(toLocation, out ITile toTile))
            {
                return false;
            }

            var moveDirection = fromTile.Location.DirectionTo(toLocation, true);

            // Try to figure out the position in the stack of the creature.
            var fromTileStackPos = fromTile.GetIndexOfThing(creature);

            if (fromTileStackPos == byte.MaxValue)
            {
                // couldn't find this creature in this tile...
                return false;
            }

            // Do the actual move first.
            if (!fromTile.RemoveCreature(creature))
            {
                return false;
            }

            if (!toTile.AddCreature(creature))
            {
                // attempt to rollback state.
                if (!fromTile.RemoveCreature(creature))
                {
                    // Leaves us in a really bad spot.
                    throw new ApplicationException("Unable to rollback state after filing to move creature. Game state is altered and inconsistent now.");
                }
            }

            var toStackPosition = toTile.GetIndexOfThing(creature);

            if (toStackPosition == byte.MaxValue)
            {
                context.Logger.LogWarning("Unexpected destination stack position during creature movement, suggests that the creature is not found in the destination tile after the move.");
            }

            // Then deal with the consequences of the move.
            ((Creature)creature).Direction = moveDirection;
            ((Creature)creature).LastMovementCostModifier = (fromTile.Location - toLocation).Z != 0 ? 2 : moveDirection.IsDiagonal() ? 3 : 1;

            this.ExhaustionInfo[ExhaustionFlag.Movement] = creature.CalculateStepDuration(fromTile);

            this.SendNotification(
                context,
                new CreatureMovedNotification(
                    context.Map.FindPlayersThatCanSee(fromTile.Location, toLocation),
                    context.Map,
                    creature,
                    fromTile.Location,
                    fromTileStackPos,
                    toTile.Location,
                    toStackPosition,
                    isTeleport));

            return true;
        }
    }
}
