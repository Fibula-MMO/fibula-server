// -----------------------------------------------------------------
// <copyright file="GatewayProtocol_v772.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772
{
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Protocol.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a gateway protocol for version 7.72.
    /// </summary>
    public class GatewayProtocol_v772 : IProtocol
    {
        /// <summary>
        /// Stores a reference to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The known packet readers to pick from.
        /// </summary>
        private readonly IDictionary<InboundPacketType, IPacketReader> packetReadersMap;

        /// <summary>
        /// The known packet writers to pick from.
        /// </summary>
        private readonly IDictionary<OutboundPacketType, IPacketWriter> packetWritersMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayProtocol_v772"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public GatewayProtocol_v772(ILogger<GatewayProtocol_v772> logger)
        {
            logger.ThrowIfNull(nameof(logger));

            this.logger = logger;

            this.packetReadersMap = new Dictionary<InboundPacketType, IPacketReader>();
            this.packetWritersMap = new Dictionary<OutboundPacketType, IPacketWriter>();
        }

        /// <summary>
        /// Registers a packet reader to this protocol.
        /// </summary>
        /// <param name="forType">The type of packet to register for.</param>
        /// <param name="packetReader">The packet reader to register.</param>
        public void RegisterPacketReader(InboundPacketType forType, IPacketReader packetReader)
        {
            packetReader.ThrowIfNull(nameof(packetReader));

            if (this.packetReadersMap.ContainsKey(forType))
            {
                throw new InvalidOperationException($"There is already a reader registered for the packet type {forType}.");
            }

            this.logger.LogTrace($"Registered packet reader for type {forType}.");

            this.packetReadersMap[forType] = packetReader;
        }

        /// <summary>
        /// Registers a packet writer to this protocol.
        /// </summary>
        /// <param name="forType">The type of packet to register for.</param>
        /// <param name="packetWriter">The packet writer to register.</param>
        public void RegisterPacketWriter(OutboundPacketType forType, IPacketWriter packetWriter)
        {
            packetWriter.ThrowIfNull(nameof(packetWriter));

            if (this.packetWritersMap.ContainsKey(forType))
            {
                throw new InvalidOperationException($"There is already a writer registered for the packet type: {forType}.");
            }

            this.logger.LogTrace($"Registered packet writer for type {forType}.");

            this.packetWritersMap[forType] = packetWriter;
        }

        /// <summary>
        /// Selects the most appropriate packet reader for the specified type.
        /// </summary>
        /// <param name="forPacketType">The type of packet.</param>
        /// <returns>An instance of an <see cref="IPacketReader"/> implementation.</returns>
        public IPacketReader SelectPacketReader(InboundPacketType forPacketType)
        {
            if (this.packetReadersMap.TryGetValue(forPacketType, out IPacketReader reader))
            {
                return reader;
            }

            return null;
        }

        /// <summary>
        /// Selects the most appropriate packet writer for the specified type.
        /// </summary>
        /// <param name="forPacketType">The type of packet.</param>
        /// <returns>An instance of an <see cref="IPacketWriter"/> implementation.</returns>
        public IPacketWriter SelectPacketWriter(OutboundPacketType forPacketType)
        {
            if (this.packetWritersMap.TryGetValue(forPacketType, out IPacketWriter writer))
            {
                return writer;
            }

            return null;
        }

        /// <summary>
        /// Attempts to convert a byte value into an <see cref="InboundPacketType"/>.
        /// </summary>
        /// <param name="fromByte">The byte to convert.</param>
        /// <returns>The <see cref="InboundPacketType"/> value converted to.</returns>
        public InboundPacketType ByteToIncomingPacketType(byte fromByte)
        {
            return fromByte switch
            {
                0x01 => InboundPacketType.LogIn,
                0xFF => InboundPacketType.ServerStatus,
                _ => InboundPacketType.Unsupported,
            };
        }
    }
}
