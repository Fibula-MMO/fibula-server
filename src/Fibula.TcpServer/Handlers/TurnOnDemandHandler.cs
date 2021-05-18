// -----------------------------------------------------------------
// <copyright file="TurnOnDemandHandler.cs" company="2Dudes">
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
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Abstract class that represents the player turning to a direction handler.
    /// </summary>
    public sealed class TurnOnDemandHandler : BaseTcpRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TurnOnDemandHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public TurnOnDemandHandler(ILogger<TurnOnDemandHandler> logger)
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

            if (!(incomingPacket is ITurnOnDemandInfo turnOnDemandInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(ITurnOnDemandInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            // TODO: cancel other pending actions.
            server.RequestToTurnCreatureAsync(client.PlayerId, turnOnDemandInfo.Direction);

            return null;
        }
    }
}
