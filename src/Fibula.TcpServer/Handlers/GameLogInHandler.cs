// -----------------------------------------------------------------
// <copyright file="GameLogInHandler.cs" company="2Dudes">
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
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Enumerations;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a character log in request handler for the game server.
    /// </summary>
    public sealed class GameLogInHandler : BaseTcpRequestHandler
    {
        /// <summary>
        /// A reference to the map of clients.
        /// </summary>
        private readonly IClientsManager clientsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameLogInHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger to use in this handler.</param>
        /// <param name="clientsMap">A reference to the clients map.</param>
        public GameLogInHandler(ILogger<GameLogInHandler> logger, IClientsManager clientsMap)
            : base(logger)
        {
            clientsMap.ThrowIfNull(nameof(clientsMap));

            this.clientsManager = clientsMap;
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

            if (!(incomingPacket is IGameLogInInfo loginInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(IGameLogInInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            if (!(client.Connection is ISocketConnection socketConnection))
            {
                this.Logger.LogError($"Expected a {nameof(ISocketConnection)} got a {client.Connection.GetType().Name}.");

                return null;
            }

            // Associate the xTea key to allow future validate packets from this connection.
            socketConnection.SetupAuthenticationKey(loginInfo.XteaKey);

            // TODO: possibly a friendly name conversion here. Also, the actual values might change per version, so this really should be set by the packet reader.
            client.Type = Enum.IsDefined(typeof(AgentType), loginInfo.ClientOs) ? (AgentType)loginInfo.ClientOs : AgentType.Windows;
            client.Version = loginInfo.ClientVersion.ToString();

            if (loginInfo.ClientVersion != server.Options.SupportedClientVersion.Numeric)
            {
                this.Logger.LogInformation($"Client attempted to connect with version: {loginInfo.ClientVersion}, OS: {loginInfo.ClientOs}. Expected version: {server.Options.SupportedClientVersion.Numeric}.");

                // TODO: hardcoded messages.
                return new GameServerDisconnectPacket($"You need client version {server.Options.SupportedClientVersion.Description} to connect to this server.").YieldSingleItem();
            }

            var (playerId, error) = server.RequestPlayerLogIn(loginInfo.AccountIdentifier, loginInfo.Password, loginInfo.CharacterName);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return new GatewayServerDisconnectPacket(error).YieldSingleItem();
            }

            client.PlayerId = playerId;

            this.clientsManager.Register(client);

            // We don't return anything synchronously in this particular case, the game will send out the login notifications when they are ready.
            return null;
        }
    }
}
