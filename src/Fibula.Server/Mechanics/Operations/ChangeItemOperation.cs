// -----------------------------------------------------------------
// <copyright file="ChangeItemOperation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics.Operations
{
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Mechanics.Notifications;

    /// <summary>
    /// Class that represents an event for an item change.
    /// </summary>
    public class ChangeItemOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeItemOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the change.</param>
        /// <param name="typeId">The type id of the item being changed.</param>
        /// <param name="fromLocation">The location from which the item is being changed.</param>
        /// <param name="toTypeId">The type id of the item to change to.</param>
        /// <param name="carrierCreature">The creature who is carrying the thing, if any.</param>
        public ChangeItemOperation(
            uint requestorId,
            ushort typeId,
            Location fromLocation,
            ushort toTypeId,
            ICreature carrierCreature = null)
            : base(requestorId)
        {
            this.FromLocation = fromLocation;
            this.FromTypeId = typeId;
            this.FromCreature = carrierCreature;
            this.ToTypeId = toTypeId;
        }

        /// <summary>
        /// Gets the location from which the item is being changed.
        /// </summary>
        public Location FromLocation { get; }

        /// <summary>
        /// Gets the type id from which the item is being changed.
        /// </summary>
        public ushort FromTypeId { get; }

        /// <summary>
        /// Gets the creature from which the item is being changed, if any.
        /// </summary>
        public ICreature FromCreature { get; }

        /// <summary>
        /// Gets the type id of the item to change to.
        /// </summary>
        public ushort ToTypeId { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            const byte FallbackIndex = 0xFF;

            var inThingContainer = this.FromLocation.DecodeContainer(context.Map, context.ContainerManager, out byte index, this.FromCreature);

            // Adjust index if this a map location.
            var existingThing = (this.FromLocation.Type == LocationType.Map && (inThingContainer is ITile fromTile)) ? fromTile.FindItemWithTypeId(this.FromTypeId) : inThingContainer?.FindThingAtIndex(index);

            if (existingThing == null || !(existingThing is IItem existingItem))
            {
                // Silent fail.
                return;
            }

            IThing thingCreated = context.ItemFactory.Create(ItemCreationArguments.WithTypeId(this.ToTypeId));

            if (thingCreated == null)
            {
                return;
            }

            // At this point, we have an item to change, and we were able to generate the new one, let's proceed.
            (bool replaceSuccessful, IThing replaceRemainder) = inThingContainer.ReplaceContent(context.ItemFactory, existingThing, thingCreated, index, existingItem.Amount);

            if (!replaceSuccessful || replaceRemainder != null)
            {
                context.GameApi.AddContentToContainerOrFallback(inThingContainer, ref replaceRemainder, FallbackIndex, includeTileAsFallback: true, this.GetRequestor(context.CreatureFinder));
            }

            if (replaceSuccessful)
            {
                if (inThingContainer is ITile atTile)
                {
                    this.SendNotification(
                        context,
                        new TileUpdatedNotification(
                            () => context.Map.FindPlayersThatCanSee(atTile.Location),
                            atTile.Location,
                            context.MapDescriptor.DescribeTile));

                    // Evaluate if the new item triggers a collision.
                    // context.EventRulesApi.EvaluateRules(this, EventRuleType.Collision, new CollisionEventRuleArguments(fromCylinder.Location, existingThing, this.GetRequestor(context.CreatureFinder)));
                }
            }
        }
    }
}
