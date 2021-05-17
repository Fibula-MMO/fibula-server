// -----------------------------------------------------------------
// <copyright file="HandlerSelector.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer
{
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a handler selector for incoming packets.
    /// </summary>
    public class HandlerSelector : IHandlerSelector
    {
        /// <summary>
        /// Stores a reference to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The map of packet types to the handler to use.
        /// </summary>
        private readonly IDictionary<Type, ITcpRequestHandler> handlersMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerSelector"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public HandlerSelector(ILogger<HandlerSelector> logger)
        {
            logger.ThrowIfNull(nameof(logger));

            this.logger = logger;
            this.handlersMap = new Dictionary<Type, ITcpRequestHandler>();
        }

        /// <summary>
        /// Registers a handler for a given packet type.
        /// </summary>
        /// <param name="packetType">The type of packet to register for.</param>
        /// <param name="handler">The handler to register.</param>
        public void RegisterForPacketType(Type packetType, ITcpRequestHandler handler)
        {
            packetType.ThrowIfNull(nameof(packetType));
            handler.ThrowIfNull(nameof(handler));

            if (this.handlersMap.ContainsKey(packetType))
            {
                throw new InvalidOperationException($"There is already a handler registered for type: {packetType}.");
            }

            this.logger.LogTrace($"Registered packet writer for type {packetType}.");

            this.handlersMap[packetType] = handler;
        }

        /// <summary>
        /// Returns the most appropriate handler for the specified packet type.
        /// </summary>
        /// <param name="packet">The packet to select the handler for.</param>
        /// <returns>An instance of an <see cref="ITcpRequestHandler"/> implementaion.</returns>
        public ITcpRequestHandler SelectForPacket(IInboundPacket packet)
        {
            packet.ThrowIfNull(nameof(packet));

            var packetType = packet.GetType();

            if (this.handlersMap.TryGetValue(packetType, out ITcpRequestHandler handler))
            {
                return handler;
            }

            foreach (var iType in packetType.GetInterfaces())
            {
                if (this.handlersMap.TryGetValue(iType, out handler))
                {
                    return handler;
                }
            }

            this.logger.LogWarning($"No handler found for type {packetType}.");

            return null;
        }
    }
}
