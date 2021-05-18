// -----------------------------------------------------------------
// <copyright file="CreatureSpeechPacketWriter.cs" company="2Dudes">
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
    /// Class that represents a creature speech packet writer for the game server.
    /// </summary>
    public class CreatureSpeechPacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureSpeechPacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public CreatureSpeechPacketWriter(ILogger<CreatureSpeechPacketWriter> logger)
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
            if (!(packet is CreatureSpeechPacket creatureSpeechPacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            message.AddByte(creatureSpeechPacket.PacketType.ToByte());

            message.AddUInt32(0);
            message.AddString(creatureSpeechPacket.SenderName);

            message.AddByte(creatureSpeechPacket.SpeechType switch
            {
                // ChannelRed = 0x05,   //Talk red on chat - #c
                // PrivateRed = 0x04,   //Red private - @name@ text
                // ChannelOrange = 0x05,    //Talk orange on text
                // ChannelRedAnonymous = 0x05,  //Talk red anonymously on chat - #d
                // MonsterYell = 0x0E,  //Yell orange
                SpeechType.Normal => 0x01,
                SpeechType.Whisper => 0x02,
                SpeechType.Yell => 0x03,
                SpeechType.Private => 0x04,
                SpeechType.ChannelYellow => 0x05,
                SpeechType.RuleViolationReport => 0x06,
                SpeechType.RuleViolationAnswer => 0x07,
                SpeechType.RuleViolationContinue => 0x08,
                SpeechType.Broadcast => 0x09,
                SpeechType.MonsterNormal => 0x0E,
                _ => 0x01,
            });

            switch (creatureSpeechPacket.SpeechType)
            {
                case SpeechType.Normal:
                case SpeechType.Whisper:
                case SpeechType.Yell:
                case SpeechType.MonsterNormal:
                // case SpeechType.MonsterYell:
                    message.AddLocation(creatureSpeechPacket.Location);
                    break;

                // case SpeechType.ChannelRed:
                // case SpeechType.ChannelRedAnonymous:
                // case SpeechType.ChannelOrange:
                // case SpeechType.ChannelWhite:
                case SpeechType.ChannelYellow:
                    message.AddUInt16(creatureSpeechPacket.Channel switch
                    {
                        ChatChannelType.RuleViolations => 0x03,
                        ChatChannelType.Default => 0x04,
                        ChatChannelType.Trade => 0x05,
                        ChatChannelType.RealLife => 0x06,
                        ChatChannelType.Help => 0x08,
                        _ => 0x04,
                    });
                    break;

                case SpeechType.RuleViolationReport:
                    message.AddUInt32(creatureSpeechPacket.Time);
                    break;

                default:
                    break;
            }

            message.AddString(creatureSpeechPacket.Text);
        }
    }
}
