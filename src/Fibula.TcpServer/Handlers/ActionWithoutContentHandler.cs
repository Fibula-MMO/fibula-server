// -----------------------------------------------------------------
// <copyright file="ActionWithoutContentHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Handlers
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a request handler for actions with no content to be read, for the game server.
    /// </summary>
    public sealed class ActionWithoutContentHandler : BaseTcpRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionWithoutContentHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public ActionWithoutContentHandler(ILogger<ActionWithoutContentHandler> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Handles the contents of a network message.
        /// </summary>
        /// <param name="server">A reference to the tcp server instance which owns the listener at which this request landed.</param>
        /// <param name="incomingPacket">The packet to handle.</param>
        /// <param name="client">A reference to the client from where this request originated from, for context.</param>
        /// <returns>A collection of <see cref="IOutboundPacket"/>s that compose that synchronous response, if any.</returns>
        public override IEnumerable<IOutboundPacket> HandleRequestPacket(ITcpServer server, IInboundPacket incomingPacket, IClient client)
        {
            server.ThrowIfNull(nameof(server));
            incomingPacket.ThrowIfNull(nameof(incomingPacket));
            client.ThrowIfNull(nameof(client));

            if (!(incomingPacket is IActionWithoutContentInfo actionInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(IActionWithoutContentInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            switch (actionInfo.Action)
            {
                case InboundPacketType.AutoMoveCancel:
                    server.RequestToCancelPlayerOperationsAsync(client.PlayerId, OperationCategory.Movement);
                    break;
                case InboundPacketType.HeartbeatResponse:
                    // NO-OP.
                    break;
                case InboundPacketType.Heartbeat:
                    server.SendHeartbeatResponseAsync(client);
                    break;
                case InboundPacketType.LogOut:
                    server.RequestPlayerLogOutAsync(client.PlayerId);
                    break;
                case InboundPacketType.StartOutfitChange:
                    // this.Game.RequestPlayerOutfitChange(client.PlayerId);
                    break;
                case InboundPacketType.StopAllActions:
                    server.RequestToCancelPlayerOperationsAsync(client.PlayerId);
                    break;
            }

            return null;
        }
    }
}
