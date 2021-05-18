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

namespace Fibula.ServerV2.Notifications
{
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for when a creature has turned.
    /// </summary>
    public class CreatureTurnedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureTurnedNotification"/> class.
        /// </summary>
        /// <param name="spectators">The spectators for the creature turning.</param>
        /// <param name="creature">The creature that turned.</param>
        /// <param name="creatureStackPosition">The position in the stack of the creature that turned.</param>
        /// <param name="turnEffect">Optional. An effect of the turn.</param>
        public CreatureTurnedNotification(IEnumerable<IPlayer> spectators, ICreature creature, byte creatureStackPosition, AnimatedEffect turnEffect = AnimatedEffect.None)
            : base(spectators)
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
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            var packets = new List<IOutboundPacket>();

            if (this.TurnedEffect != AnimatedEffect.None)
            {
                packets.Add(new MagicEffectPacket(this.Creature.Location, this.TurnedEffect));
            }

            packets.Add(new CreatureTurnedPacket(this.Creature, this.StackPosition));

            return packets;
        }
    }
}
