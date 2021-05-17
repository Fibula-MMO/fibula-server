// -----------------------------------------------------------------
// <copyright file="CreatureTurnedNotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for when a creature has turned.
    /// </summary>
    public class CreatureTurnedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureTurnedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="creature">The creature that turned.</param>
        /// <param name="creatureStackPosition">The position in the stack of the creature that turned.</param>
        /// <param name="turnEffect">Optional. An effect of the turn.</param>
        public CreatureTurnedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, ICreature creature, byte creatureStackPosition, AnimatedEffect turnEffect = AnimatedEffect.None)
            : base(findTargetPlayers)
        {
            creature.ThrowIfNull(nameof(creature));

            this.Creature = creature;
            this.StackPosition = creatureStackPosition;
            this.TurnedEffect = turnEffect;
        }

        /// <summary>
        /// Gets the creature that turned.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Gets the position in the stack of the creatue.
        /// </summary>
        public byte StackPosition { get; }

        /// <summary>
        /// Gets the effect of the turn, if any.
        /// </summary>
        public AnimatedEffect TurnedEffect { get; }

        /// <summary>
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>True if the notification was posted successfuly, and false otherwise.</returns>
        public override bool Post(INotificationContext context, IPlayer player)
        {
            if (!(context.Buffer is ITargetBlock<GameNotification> targetBuffer))
            {
                return false;
            }

            if (this.TurnedEffect != AnimatedEffect.None)
            {
                targetBuffer.Post(
                    new GameNotification()
                    {
                        MagicEffect = new MagicEffect()
                        {
                            Effect = (uint)this.TurnedEffect,
                            Location = new Common.Contracts.Grpc.Location()
                            {
                                X = (ulong)this.Creature.Location.X,
                                Y = (ulong)this.Creature.Location.Y,
                                Z = (uint)this.Creature.Location.Z,
                            },
                        },
                    });
            }

            return targetBuffer.Post(
                new GameNotification()
                {
                    CreatureTurned = new CreatureTurned()
                    {
                        AtLocation = new Common.Contracts.Grpc.Location()
                        {
                            X = (ulong)this.Creature.Location.X,
                            Y = (ulong)this.Creature.Location.Y,
                            Z = (uint)this.Creature.Location.Z,
                        },
                        Index = this.StackPosition,
                        ThingId = this.Creature.TypeId,
                        CreatureId = this.Creature.Id,
                        Direction = (uint)this.Creature.Direction,
                    },
                });
        }
    }
}
