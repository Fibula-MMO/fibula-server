﻿// -----------------------------------------------------------------
// <copyright file="TextMessagePacketWriter.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772.PacketWriters
{
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Enumerations;
    using Fibula.Protocol.V772.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a text message packet writer for the game server.
    /// </summary>
    public class TextMessagePacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextMessagePacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public TextMessagePacketWriter(ILogger<TextMessagePacketWriter> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Writes a packet to the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="packet">The packet to write.</param>
        /// <param name="message">The message to write into.</param>
        public override void WriteToMessage(IOutboundPacket packet, ref INetworkMessage message)
        {
            if (!(packet is TextMessagePacket textMessagePacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            message.AddByte(textMessagePacket.PacketType.ToByte());

            byte valueToSend = textMessagePacket.Type switch
            {
                MessageType.CenterWhite => 0x13,
                MessageType.CenterGreen => 0x16,
                MessageType.CenterRed => 0x12,
                MessageType.Status => 0x15,
                MessageType.StatusNoConsole => 0x17,
                MessageType.ConsoleOnlyBlue => 0x18,
                MessageType.ConsoleOnlyRed => 0x19,
                MessageType.ConsoleOnlyYellow => 0x01,
                MessageType.ConsoleOnlyLightBlue => 0x04,
                MessageType.ConsoleOnlyOrange => 0x11,
                _ => 0x14,
            };

            message.AddByte(valueToSend);
            message.AddString(textMessagePacket.Message);
        }
    }
}
