// -----------------------------------------------------------------
// <copyright file="Operation.cs" company="2Dudes">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Flags;
    using Fibula.Scheduling;
    using Fibula.Scheduling.Contracts.Abstractions;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Mechanics.Conditions;
    using Fibula.Server.Notifications;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a common base between game operations.
    /// </summary>
    public abstract class Operation : BaseEvent, IOperation
    {
        /// <summary>
        /// Caches the requestor creature, if defined.
        /// </summary>
        private ICreature requestor = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Operation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the movement.</param>
        protected Operation(uint requestorId)
            : base(requestorId)
        {
            this.CanBeCancelled = true;

            this.ExhaustionInfo = new Dictionary<ExhaustionFlag, TimeSpan>();
        }

        /// <summary>
        /// Gets a string representing this operation's event type.
        /// </summary>
        public override string EventType => this.GetType().Name;

        /// <summary>
        /// Gets or sets a value indicating whether the event can be cancelled.
        /// </summary>
        public override bool CanBeCancelled { get; protected set; }

        /// <summary>
        /// Gets the exhaustion conditions that this operation checks for and produces.
        /// </summary>
        public IDictionary<ExhaustionFlag, TimeSpan> ExhaustionInfo { get; }

        /// <summary>
        /// Gets the creature that is requesting the event, if known.
        /// </summary>
        /// <param name="creatureFinder">A reference to the creature finder in use.</param>
        /// <returns>The creature that requested the operation, or null if there wasn't any.</returns>
        public ICreature GetRequestor(ICreatureFinder creatureFinder)
        {
            creatureFinder.ThrowIfNull(nameof(creatureFinder));

            if (this.requestor == null && this.RequestorId > 0)
            {
                this.requestor = creatureFinder.FindCreatureById(this.RequestorId);
            }

            return this.requestor;
        }

        /// <summary>
        /// Executes the event logic.
        /// </summary>
        /// <param name="context">The execution context.</param>
        public override void Execute(IEventContext context)
        {
            context.ThrowIfNull(nameof(context));

            if (!typeof(IOperationContext).IsAssignableFrom(context.GetType()))
            {
                throw new ArgumentException($"{nameof(context)} must be an {nameof(IOperationContext)}.");
            }

            var operationContext = context as IOperationContext;

            // Reset the operation's Repeat property, to avoid implementations running perpetually.
            this.RepeatAfter = TimeSpan.MinValue;

            this.Execute(operationContext);

            // Add any associated exhaustion from this operation, the the requestor, if there was one.
            if (this.ExhaustionInfo.Any() && this.GetRequestor(operationContext.CreatureManager) is ICreature requestor)
            {
                foreach (var (exhaustionType, duration) in this.ExhaustionInfo)
                {
                    var exhaustionCondition = new ExhaustionCondition(exhaustionType, context.CurrentTime + duration);

                    operationContext.GameApi.AddOrAggregateCondition(requestor, exhaustionCondition, duration);
                }
            }
        }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">The execution context for this operation.</param>
        protected abstract void Execute(IOperationContext context);

        /// <summary>
        /// Sends a <see cref="TextMessageNotification"/> to the requestor of the operation, if there is one and it is a player.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        /// <param name="message">Optional. The message to send. Defaults to <see cref="OperationMessage.NotPossible"/>.</param>
        protected void DispatchTextNotification(IOperationContext context, string message = OperationMessage.NotPossible)
        {
            if (this.RequestorId == 0 || !(context.CreatureManager.FindCreatureById(this.RequestorId) is IPlayer player))
            {
                return;
            }

            message.ThrowIfNullOrWhiteSpace();

            this.SendNotification(context, new TextMessageNotification(() => player.YieldSingleItem(), MessageType.StatusSmall, message));
        }

        /// <summary>
        /// Sends a notification synchronously.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        /// <param name="notification">The notification to send.</param>
        protected void SendNotification(IOperationContext context, INotification notification)
        {
            notification.ThrowIfNull(nameof(notification));

            context.GameApi.SendNotification(notification);
        }
    }
}
