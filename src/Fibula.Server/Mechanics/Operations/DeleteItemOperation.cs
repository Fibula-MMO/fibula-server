// -----------------------------------------------------------------
// <copyright file="DeleteItemOperation.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Mechanics.Notifications;

    /// <summary>
    /// Class that represents an event for an item deletion.
    /// </summary>
    public class DeleteItemOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteItemOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the deletion.</param>
        /// <param name="item">The item that's being deleted.</param>
        public DeleteItemOperation(uint requestorId, IItem item)
            : base(requestorId)
        {
            this.Item = item;
        }

        /// <summary>
        /// Gets the item that is expiring.
        /// </summary>
        public IItem Item { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            var requestor = this.GetRequestor(context.CreatureFinder);
            var inThingContainer = this.Item.ParentContainer;

            if (inThingContainer == null || !(this.Item is IThing existingThing))
            {
                return;
            }

            // At this point, we have an item to remove, let's proceed.
            (bool removeSuccessful, IThing remainder) = inThingContainer.RemoveContent(context.ItemFactory, ref existingThing, amount: this.Item.Amount);

            if (!removeSuccessful)
            {
                // Failing to remove the item from the original cylinder stops the entire operation.
                return;
            }

            if (inThingContainer is ITile atTile)
            {
                this.SendNotification(
                    context,
                    new TileUpdatedNotification(
                        () => context.Map.FindPlayersThatCanSee(atTile.Location),
                        atTile.Location,
                        context.MapDescriptor.DescribeTile));
            }

            // Evaluate if the new item triggers a separation.
            // context.EventRulesApi.EvaluateRules(this, EventRuleType.Separation, new SeparationEventRuleArguments(atCylinder.Location, item, this.GetRequestor(context.CreatureFinder)));
        }
    }
}
