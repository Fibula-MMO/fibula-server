﻿// -----------------------------------------------------------------
// <copyright file="DecayingCondition.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Conditions
{
    using System;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Contracts.Models;
    using Fibula.Server.Notifications;
    using Fibula.Server.Operations;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents an event for an item expiring.
    /// </summary>
    public class DecayingCondition : Condition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingCondition"/> class.
        /// </summary>
        /// <param name="item">The item that's decaying.</param>
        public DecayingCondition(IItem item)
            : base(ConditionType.Decaying)
        {
            this.Item = item;
        }

        /// <summary>
        /// Gets the item that is decaying.
        /// </summary>
        public IItem Item { get; }

        /// <summary>
        /// Aggregates this condition into another of the same type.
        /// </summary>
        /// <param name="conditionOfSameType">The condition to aggregate into.</param>
        /// <returns>True if the conditions were aggregated (changed), and false if nothing was done.</returns>
        public override bool Aggregate(ICondition conditionOfSameType)
        {
            conditionOfSameType.ThrowIfNull(nameof(conditionOfSameType));

            if (!(conditionOfSameType is DecayingCondition otherDecayCondition) || otherDecayCondition.Item.TypeId != this.Item.TypeId)
            {
                return false;
            }

            // Nothing to aggregate between these conditions, but we return true because we want to evaluate
            // if the current condition's end should be delayed.
            return true;
        }

        /// <summary>
        /// Executes the condition's logic.
        /// </summary>
        /// <param name="context">The execution context for this condition.</param>
        protected override void Execute(IConditionContext context)
        {
            var inThingContainer = this.Item.ParentContainer;

            if (this.Item == null || !this.Item.HasExpiration || inThingContainer == null)
            {
                // Silent fail.
                return;
            }

            if (this.Item.ExpirationTarget == 0)
            {
                // We will delete this item.
                context.Scheduler.ScheduleEvent(new DeleteItemOperation(requestorId: 0, this.Item));

                return;
            }

            var creationArguments = ItemCreationArguments.WithTypeId(this.Item.ExpirationTarget);

            if (this.Item.IsLiquidPool)
            {
                creationArguments.Attributes = new[]
                {
                    (ItemAttribute.LiquidType, this.Item.LiquidType as IConvertible),
                };
            }

            IItem itemCreated = context.ItemFactory.CreateItem(creationArguments);

            if (itemCreated == null)
            {
                return;
            }

            // At this point, we have an item to change, and we were able to generate the new one, let's proceed.
            (bool replaceSuccessful, IThing replaceRemainder) = inThingContainer.ReplaceItem(context.ItemFactory, this.Item, itemCreated, byte.MaxValue, this.Item.Amount);

            if (replaceSuccessful)
            {
                if (inThingContainer is ITile atTile)
                {
                    this.SendNotification(
                        context,
                        new TileUpdatedNotification(context.Map.FindPlayersThatCanSee(atTile.Location), atTile));

                    // TODO: Evaluate if the new item triggers a collision.
                    // context.EventRulesApi.EvaluateRules(this, EventRuleType.Collision, new CollisionEventRuleArguments(fromCylinder.Location, existingThing, this.GetRequestor(context.CreatureFinder)));
                }
            }
        }
    }
}
