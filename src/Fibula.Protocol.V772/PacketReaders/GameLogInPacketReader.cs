// -----------------------------------------------------------------
// <copyright file="GameLogInPacketReader.cs" company="2Dudes">
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
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Incoming;
    using Fibula.Security.Contracts;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a log in packet reader for the game server.
    /// </summary>
    public sealed class GameLogInPacketReader : BasePacketReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameLogInPacketReader"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="applicationContext">A reference to the application context.</param>
        /// <param name="rsaDecryptor">A reference to the RSA decryptor in use.</param>
        public GameLogInPacketReader(
            ILogger<GameLogInPacketReader> logger,
            IApplicationContext applicationContext,
            IRsaDecryptor rsaDecryptor)
            : base(logger)
        {
            applicationContext.ThrowIfNull(nameof(applicationContext));
            rsaDecryptor.ThrowIfNull(nameof(rsaDecryptor));

            this.ApplicationContext = applicationContext;
            this.RsaDecryptor = rsaDecryptor;
        }

        /// <summary>
        /// Gets a reference to the application context.
        /// </summary>
        public IApplicationContext ApplicationContext { get; }

        /// <summary>
        /// Gets the RSA decryptor to use.
        /// </summary>
        public IRsaDecryptor RsaDecryptor { get; }

        /// <summary>
        /// Reads a packet from the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="message">The message to read from.</param>
        /// <returns>The packet read from the message.</returns>
        public override IIncomingPacket ReadFromMessage(INetworkMessage message)
        {
            message.ThrowIfNull(nameof(message));

            // OS and Version were included in plain in this packet after version 7.70
            var operatingSystem = message.GetUInt16();
            var version = message.GetUInt16();

            var targetSpan = message.Buffer[message.Cursor..message.Length];

            this.RsaDecryptor.Decrypt(targetSpan);

            // Note: there are 92 bytes that follow the password, and I have no idea what they contain.
            return new GameLogInPacket(
                xteaKey: new uint[]
                {
                    message.GetUInt32(),
                    message.GetUInt32(),
                    message.GetUInt32(),
                    message.GetUInt32(),
                },
                operatingSystem: operatingSystem,       // OS was only included in this packet before version 7.71
                version: version,                       // Version was only included in this packet before version 7.71
                isGamemaster: message.GetByte() > 0,
                accountNumber: message.GetUInt32(),
                characterName: message.GetString(),
                password: message.GetString());
        }
    }
}
