// -----------------------------------------------------------------
// <copyright file="PlayerLogoutPacket.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a player logout packet.
    /// </summary>
    public class PlayerLogoutPacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLogoutPacket"/> class.
        /// </summary>
        /// <param name="player">A reference to the player.</param>
        public PlayerLogoutPacket(IPlayer player)
        {
            this.Player = player;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType => OutboundPacketType.PlayerLogout;

        /// <summary>
        /// Gets a reference to the player.
        /// </summary>
        public IPlayer Player { get; }
    }
}
