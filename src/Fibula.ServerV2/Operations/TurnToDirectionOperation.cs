// -----------------------------------------------------------------
// <copyright file="TurnToDirectionOperation.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Extensions;
    using Fibula.ServerV2.Creatures;
    using Fibula.ServerV2.Notifications;

    /// <summary>
    /// Class that represents an event for a creature turning.
    /// </summary>
    public class TurnToDirectionOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TurnToDirectionOperation"/> class.
        /// </summary>
        /// <param name="creature">The creature which is turning.</param>
        /// <param name="direction">The direction to which the creature is turning.</param>
        public TurnToDirectionOperation(ICreature creature, Direction direction)
            : base(creature.Id)
        {
            this.Creature = creature;
            this.Direction = direction;
        }

        /// <summary>
        /// Gets a reference to the creature turning.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Gets the direction in which the creature is turning.
        /// </summary>
        public Direction Direction { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            // Perform the actual, internal turn.
            ((Creature)this.Creature).Direction = this.Direction;

            // Send the notification if applicable.
            if (context.Map.HasTileAt(this.Creature.Location, out ITile playerTile))
            {
                var playerStackPos = playerTile.GetIndexOfThing(this.Creature);

                var notification = new CreatureTurnedNotification(context.Map.FindPlayersThatCanSee(this.Creature.Location), this.Creature, playerStackPos);

                this.SendNotification(context, notification);
            }
        }
    }
}
