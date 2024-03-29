﻿// -----------------------------------------------------------------
// <copyright file="BaseTcpRequestHandler.cs" company="2Dudes">
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
    /// Class that represents the base implementation for all request handlers in all TCP protocols.
    /// </summary>
    public abstract class BaseTcpRequestHandler : ITcpRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTcpRequestHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        protected BaseTcpRequestHandler(ILogger logger)
        {
            logger.ThrowIfNull(nameof(logger));

            this.Logger = logger;
        }

        /// <summary>
        /// Gets the reference to the logger in use.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Handles the contents of a request packet.
        /// </summary>
        /// <param name="tcpServerInstance">A reference to the tcp server instance which owns the listener at which this request landed.</param>
        /// <param name="incomingPacket">The packet to handle.</param>
        /// <param name="client">A reference to the client from where this request originated from, for context.</param>
        /// <returns>A collection of <see cref="IOutboundPacket"/>s that compose that synchronous response, if any.</returns>
        public abstract IEnumerable<IOutboundPacket> HandleRequestPacket(ITcpServer tcpServerInstance, IInboundPacket incomingPacket, IClient client);
    }
}
