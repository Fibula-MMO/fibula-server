﻿// -----------------------------------------------------------------
// <copyright file="PlayerLoginPacketWriter.cs" company="2Dudes">
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
    using System;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Protocol.V772.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a player login packet writer for the game server.
    /// </summary>
    public class PlayerLoginPacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLoginPacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public PlayerLoginPacketWriter(ILogger<PlayerLoginPacketWriter> logger)
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
            if (!(packet is PlayerLoginPacket playerLoginPacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            message.AddByte(playerLoginPacket.PacketType.ToByte());

            message.AddUInt32(playerLoginPacket.CreatureId);
            message.AddByte(playerLoginPacket.GraphicsSpeed);
            message.AddByte(playerLoginPacket.CanReportBugs);

            message.AddByte(Math.Min((byte)0x01, playerLoginPacket.Player.PermissionsLevel));

            if (playerLoginPacket.Player.PermissionsLevel > 0)
            {
                message.AddByte(OutboundPacketType.GamemasterFlags.ToByte());

                for (var i = 0; i < 32; i++)
                {
                    // TODO: Actually send individual permissions flags
                    message.AddByte(byte.MaxValue);
                }
            }
        }
    }
}
