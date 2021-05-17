// -----------------------------------------------------------------
// <copyright file="CreatureAddedNotification.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Constants;
    using Fibula.ServerV2.Contracts.Extensions;

    /// <summary>
    /// Class that represents a notification for when a creature has moved.
    /// </summary>
    public class CreatureAddedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureAddedNotification"/> class.
        /// </summary>
        /// <param name="spectators">The set of players that spectated the creature being added.</param>
        /// <param name="addedCreature">The creature being added.</param>
        /// <param name="atLocation">The location to which the creature is being added.</param>
        /// <param name="atIndex">The index (or position in the stack) at which the creature is being added.</param>
        /// <param name="addEffect">Optional. An effect to add when removing the creature.</param>
        public CreatureAddedNotification(
            IEnumerable<IPlayer> spectators,
            ICreature addedCreature,
            Location atLocation,
            byte atIndex,
            AnimatedEffect addEffect = AnimatedEffect.None)
            : base(spectators)
        {
            this.Creature = addedCreature;
            this.AtLocation = atLocation;
            this.AtStackPosition = atIndex;
            this.AddEffect = addEffect;
        }

        /// <summary>
        /// Gets the creature being added.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Gets the location to which the creature is being added.
        /// </summary>
        public Location AtLocation { get; }

        /// <summary>
        /// Gets the stack position to which the creature is being added.
        /// </summary>
        public byte AtStackPosition { get; }

        /// <summary>
        /// Gets the effect to send when adding the creature.
        /// </summary>
        public AnimatedEffect AddEffect { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            var packets = new List<IOutboundPacket>();

            if (player.CanSee(this.AtLocation) && player.CanSee(this.Creature))
            {
                if (this.AtStackPosition <= MapConstants.MaximumNumberOfThingsToDescribePerTile)
                {
                    packets.Add(new AddCreaturePacket(player, this.Creature));
                }
            }

            if (this.AddEffect != AnimatedEffect.None)
            {
                packets.Add(new MagicEffectPacket(this.AtLocation, this.AddEffect));
            }

            return packets;
        }
    }
}
