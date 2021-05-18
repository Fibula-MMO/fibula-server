// -----------------------------------------------------------------
// <copyright file="AttackHandler.cs" company="2Dudes">
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
    /// Class that represents a handler for attack requests.
    /// </summary>
    public sealed class AttackHandler : BaseTcpRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttackHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public AttackHandler(ILogger<AttackHandler> logger)
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

            if (!(incomingPacket is IAttackInfo attackInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(IAttackInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            // First stop following (formally) if we are.
            server.RequestToFollowCreatureAsync(client.PlayerId);

            // And then start attacking. Game will handle if that implicitly means follow the target of the attack.
            server.RequestToAttackCreatureAsync(client.PlayerId, attackInfo.TargetCreatureId);

            return null;
        }
    }
}
