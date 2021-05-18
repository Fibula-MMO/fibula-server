// -----------------------------------------------------------------
// <copyright file="HeartbeatResponsePacket.cs" company="2Dudes">
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

    /// <summary>
    /// Class that represents a packet for cancelling a player's walk.
    /// </summary>
    public class HeartbeatResponsePacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatResponsePacket"/> class.
        /// </summary>
        public HeartbeatResponsePacket()
        {
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType => OutboundPacketType.HeartbeatResponse;
    }
}
