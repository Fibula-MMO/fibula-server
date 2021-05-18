// -----------------------------------------------------------------
// <copyright file="ConnectionPacketReadyHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Contracts.Delegates
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;

    /// <summary>
    /// Delegate to handle when a packet is ready to be processed.
    /// </summary>
    /// <param name="connection">The connection from which the packet is was read.</param>
    /// <param name="packet">The packet ready to be processed.</param>
    /// <returns>A collection of <see cref="IOutboundPacket"/> to respond with as a result of processing the <see cref="IInboundPacket"/>.</returns>
    public delegate IEnumerable<IOutboundPacket> ConnectionPacketReadyHandler(IConnection connection, IInboundPacket packet);
}
