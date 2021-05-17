// -----------------------------------------------------------------
// <copyright file="GatewayLogInHandler.cs" company="2Dudes">
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
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a new connection request handler for the login server.
    /// </summary>
    public sealed class GatewayLogInHandler : BaseTcpRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayLogInHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public GatewayLogInHandler(ILogger<GatewayLogInHandler> logger)
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

            if (!(incomingPacket is IGatewayLoginInfo accountLoginInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(IGatewayLoginInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            if (!(client.Connection is ISocketConnection socketConnection))
            {
                this.Logger.LogError($"Expected a {nameof(ISocketConnection)} got a {client.Connection.GetType().Name}.");

                return null;
            }

            // Associate the xTea key to allow future validate packets from this connection.
            socketConnection.SetupAuthenticationKey(accountLoginInfo.XteaKey);

            if (accountLoginInfo.ClientVersion != server.Options.SupportedClientVersion.Numeric)
            {
                this.Logger.LogInformation($"Client attempted to connect with version: {accountLoginInfo.ClientVersion}, OS: {accountLoginInfo.ClientOs}. Expected version: {server.Options.SupportedClientVersion.Numeric}.");

                // TODO: hardcoded messages.
                return new GatewayServerDisconnectPacket($"You need client version {server.Options.SupportedClientVersion.Description} to connect to this server.").YieldSingleItem();
            }

            var (characters, premDays, error) = server.DoAccountLogin(accountLoginInfo.AccountName, accountLoginInfo.Password);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return new GatewayServerDisconnectPacket(error).YieldSingleItem();
            }

            return new IOutboundPacket[]
            {
                new MessageOfTheDayPacket(server.InformationalMessage),
                new CharacterListPacket(characters, (ushort)premDays),
            };
        }
    }
}
