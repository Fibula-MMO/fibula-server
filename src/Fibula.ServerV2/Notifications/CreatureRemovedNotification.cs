// -----------------------------------------------------------------
// <copyright file="CreatureRemovedNotification.cs" company="2Dudes">
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
    using Fibula.ServerV2.Contracts.Extensions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for when a creature is removed from the map.
    /// </summary>
    public class CreatureRemovedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureRemovedNotification"/> class.
        /// </summary>
        /// <param name="spectators">The set of players that spectated the creature removal.</param>
        /// <param name="removedCreature">The creature being removed.</param>
        /// <param name="oldStackPos">The position in the stack of the creature being removed.</param>
        /// <param name="removeEffect">Optional. An effect to add when removing the creature.</param>
        public CreatureRemovedNotification(IEnumerable<IPlayer> spectators, ICreature removedCreature, byte oldStackPos, AnimatedEffect removeEffect = AnimatedEffect.None)
            : base(spectators)
        {
            removedCreature.ThrowIfNull(nameof(removedCreature));

            this.Creature = removedCreature;
            this.StackPosition = oldStackPos;
            this.RemoveEffect = removeEffect;
        }

        /// <summary>
        /// Gets the effect to send when removing the creature.
        /// </summary>
        public AnimatedEffect RemoveEffect { get; }

        /// <summary>
        /// Gets the stack position of the creature being removed.
        /// </summary>
        public byte StackPosition { get; }

        /// <summary>
        /// Gets the creature being removed.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            if (player == null || !player.CanSee(this.Creature) || !player.CanSee(this.Creature.Location))
            {
                return null;
            }

            var packets = new List<IOutboundPacket>
            {
                new RemoveAtLocationPacket(this.Creature.Location, this.StackPosition),
            };

            if (this.RemoveEffect != AnimatedEffect.None)
            {
                packets.Add(new MagicEffectPacket(this.Creature.Location, this.RemoveEffect));
            }

            return packets;
        }
    }
}
