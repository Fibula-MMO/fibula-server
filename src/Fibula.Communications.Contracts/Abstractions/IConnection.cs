// -----------------------------------------------------------------
// <copyright file="IConnection.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Contracts.Abstractions
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Delegates;
    using Fibula.Communications.Packets.Contracts.Abstractions;

    /// <summary>
    /// Interface for all types of connections.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Event fired when this connection has been closed.
        /// </summary>
        event ConnectionClosedHandler Closed;

        /// <summary>
        /// Event fired right after this connection has proccessed an <see cref="IInboundPacket"/> by any subscriber of the <see cref="PacketReady"/> event.
        /// </summary>
        event ConnectionPacketProccessedHandler PacketProcessed;

        /// <summary>
        /// Event fired when a <see cref="IInboundPacket"/> picked up by this connection is ready to be processed.
        /// </summary>
        event ConnectionPacketReadyHandler PacketReady;

        /// <summary>
        /// Gets a value indicating whether the connection is an orphan.
        /// </summary>
        bool IsOrphaned { get; }

        /// <summary>
        /// Reads the next message in this connection.
        /// </summary>
        void Read();

        /// <summary>
        /// Closes this connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Sends the packets supplied through this connection.
        /// </summary>
        /// <param name="packetsToSend">The packets to send.</param>
        /// <param name="playerId">Optional. The id of the player attached to the connection, for context.</param>
        void Send(IEnumerable<IOutboundPacket> packetsToSend, uint playerId = 0);
    }
}
