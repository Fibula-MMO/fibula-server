// -----------------------------------------------------------------
// <copyright file="IOutboundPacket.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Packets.Contracts.Abstractions
{
    using Fibula.Communications.Packets.Contracts.Enumerations;

    /// <summary>
    /// Interface for all outbound packets.
    /// </summary>
    public interface IOutboundPacket
    {
        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        OutboundPacketType PacketType { get; }
    }
}
