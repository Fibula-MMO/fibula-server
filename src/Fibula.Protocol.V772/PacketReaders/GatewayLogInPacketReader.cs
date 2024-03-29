﻿// -----------------------------------------------------------------
// <copyright file="GatewayLogInPacketReader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772.PacketReaders
{
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Incoming;
    using Fibula.Security.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a log in packet reader for the gateway server.
    /// </summary>
    public sealed class GatewayLogInPacketReader : BasePacketReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayLogInPacketReader"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="rsaDecryptor">A reference to the RSA decryptor in use.</param>
        public GatewayLogInPacketReader(
            ILogger<GatewayLogInPacketReader> logger,
            IRsaDecryptor rsaDecryptor)
            : base(logger)
        {
            rsaDecryptor.ThrowIfNull(nameof(rsaDecryptor));

            this.RsaDecryptor = rsaDecryptor;
        }

        /// <summary>
        /// Gets the RSA decryptor to use.
        /// </summary>
        public IRsaDecryptor RsaDecryptor { get; }

        /// <summary>
        /// Reads a packet from the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="message">The message to read from.</param>
        /// <returns>The packet read from the message.</returns>
        public override IInboundPacket ReadFromMessage(INetworkMessage message)
        {
            message.ThrowIfNull(nameof(message));

            // OS and Version were included plainly in this version
            var operatingSystem = message.GetUInt16();
            var version = message.GetUInt16();

            // Skip dat, spr, and pic signatures (4 bytes each)?
            message.SkipBytes(12);

            var targetSpan = message.Buffer[message.Cursor..message.Length];

            this.RsaDecryptor.Decrypt(targetSpan);

            return new GatewayLogInPacket(
                version,
                operatingSystem,
                xteaKey: new uint[] { message.GetUInt32(), message.GetUInt32(), message.GetUInt32(), message.GetUInt32() },
                accountIdentifier: message.GetUInt32().ToString(),
                password: message.GetString());
        }
    }
}
