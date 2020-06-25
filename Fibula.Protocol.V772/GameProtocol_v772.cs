﻿// -----------------------------------------------------------------
// <copyright file="GameProtocol_v772.cs" company="2Dudes">
// Copyright (c) 2018 2Dudes. All rights reserved.
// Author: Jose L. Nunez de Caceres
// jlnunez89@gmail.com
// http://linkedin.com/in/jlnunez89
//
// Licensed under the MIT license.
// See LICENSE.txt file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772
{
    using System;
    using System.Collections.Generic;
    using Fibula.Common.Utilities;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Contracts.Enumerations;
    using Serilog;

    /// <summary>
    /// Class that represents a game protocol for version 7.72.
    /// </summary>
    public class GameProtocol_v772 : IProtocol
    {
        /// <summary>
        /// Stores a reference to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The known packet readers to pick from.
        /// </summary>
        private readonly IDictionary<IncomingGamePacketType, IPacketReader> packetReadersMap;

        /// <summary>
        /// The known packet writers to pick from.
        /// </summary>
        private readonly IDictionary<OutgoingGamePacketType, IPacketWriter> packetWritersMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameProtocol_v772"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public GameProtocol_v772(ILogger logger)
        {
            logger.ThrowIfNull(nameof(logger));

            this.logger = logger.ForContext<GameProtocol_v772>();

            this.packetReadersMap = new Dictionary<IncomingGamePacketType, IPacketReader>();
            this.packetWritersMap = new Dictionary<OutgoingGamePacketType, IPacketWriter>();
        }

        /// <summary>
        /// Registers a packet reader to this protocol.
        /// </summary>
        /// <param name="forType">The type of packet to register for.</param>
        /// <param name="packetReader">The packet reader to register.</param>
        public void RegisterPacketReader(byte forType, IPacketReader packetReader)
        {
            packetReader.ThrowIfNull(nameof(packetReader));

            if (!Enum.IsDefined(typeof(IncomingGamePacketType), forType))
            {
                throw new InvalidOperationException($"Unsupported packet type {forType:x2}. Check the types defined in {nameof(IncomingGamePacketType)}.");
            }

            IncomingGamePacketType packetType = (IncomingGamePacketType)forType;

            if (this.packetReadersMap.ContainsKey(packetType))
            {
                throw new InvalidOperationException($"There is already a reader registered for the packet type {packetType}.");
            }

            this.logger.Verbose($"Registered packet reader for type {packetType}.");

            this.packetReadersMap[packetType] = packetReader;
        }

        /// <summary>
        /// Registers a packet writer to this protocol.
        /// </summary>
        /// <param name="forType">The type of packet to register for.</param>
        /// <param name="packetWriter">The packet writer to register.</param>
        public void RegisterPacketWriter(byte forType, IPacketWriter packetWriter)
        {
            packetWriter.ThrowIfNull(nameof(packetWriter));

            if (!Enum.IsDefined(typeof(OutgoingGamePacketType), forType))
            {
                throw new InvalidOperationException($"Unsupported packet type {forType:x2}. Check the types defined in {nameof(OutgoingGamePacketType)}.");
            }

            OutgoingGamePacketType packetType = (OutgoingGamePacketType)forType;

            if (this.packetWritersMap.ContainsKey(packetType))
            {
                throw new InvalidOperationException($"There is already a writer registered for the packet type: {packetType}.");
            }

            this.logger.Verbose($"Registered packet writer for type {packetType}.");

            this.packetWritersMap[packetType] = packetWriter;
        }

        /// <summary>
        /// Selects the most appropriate packet reader for the specified type.
        /// </summary>
        /// <param name="forPacketType">The type of packet.</param>
        /// <returns>An instance of an <see cref="IPacketReader"/> implementation.</returns>
        public IPacketReader SelectPacketReader(byte forPacketType)
        {
            if (Enum.IsDefined(typeof(IncomingGamePacketType), forPacketType) && this.packetReadersMap.TryGetValue((IncomingGamePacketType)forPacketType, out IPacketReader reader))
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
        public IPacketWriter SelectPacketWriter(byte forPacketType)
        {
            if (Enum.IsDefined(typeof(OutgoingGamePacketType), forPacketType) && this.packetWritersMap.TryGetValue((OutgoingGamePacketType)forPacketType, out IPacketWriter writer))
            {
                return writer;
            }

            return null;
        }
    }
}