// -----------------------------------------------------------------
// <copyright file="AddCreaturePacket.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Packets.Outgoing
{
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a packet with information about a creatue that was added to the game.
    /// </summary>
    public class AddCreaturePacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddCreaturePacket"/> class.
        /// </summary>
        /// <param name="player">The player that observed the creature addition.</param>
        /// <param name="creature">The creature that was added.</param>
        public AddCreaturePacket(IPlayer player, ICreature creature)
        {
            player.ThrowIfNull(nameof(player));
            creature.ThrowIfNull(nameof(creature));

            this.Player = player;
            this.Creature = creature;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType => OutboundPacketType.AddThing;

        /// <summary>
        /// Gets the reference to the player which observed the creature being added.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Gets a reference to the creature added.
        /// </summary>
        public ICreature Creature { get; }
    }
}
