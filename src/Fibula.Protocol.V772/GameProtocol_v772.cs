// -----------------------------------------------------------------
// <copyright file="GameProtocol_v772.cs" company="2Dudes">
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
        private readonly IDictionary<InboundPacketType, IPacketReader> packetReadersMap;

        /// <summary>
        /// The known packet writers to pick from.
        /// </summary>
        private readonly IDictionary<OutboundPacketType, IPacketWriter> packetWritersMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameProtocol_v772"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public GameProtocol_v772(ILogger<GameProtocol_v772> logger)
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
                0x0A => InboundPacketType.LogIn,
                0x14 => InboundPacketType.LogOut,
                0x1D => InboundPacketType.HeartbeatResponse,
                0x1E => InboundPacketType.Heartbeat,
                0x64 => InboundPacketType.AutoMove,
                0x65 => InboundPacketType.WalkNorth,
                0x66 => InboundPacketType.WalkEast,
                0x67 => InboundPacketType.WalkSouth,
                0x68 => InboundPacketType.WalkWest,
                0x69 => InboundPacketType.AutoMoveCancel,
                0x6A => InboundPacketType.WalkNortheast,
                0x6B => InboundPacketType.WalkSoutheast,
                0x6C => InboundPacketType.WalkSouthwest,
                0x6D => InboundPacketType.WalkNorthwest,
                0x6F => InboundPacketType.TurnNorth,
                0x70 => InboundPacketType.TurnEast,
                0x71 => InboundPacketType.TurnSouth,
                0x72 => InboundPacketType.TurnWest,
                0x78 => InboundPacketType.MoveThing,
                0x7D => InboundPacketType.TradeRequest,
                0x7E => InboundPacketType.TradeLook,
                0x7F => InboundPacketType.TradeAccept,
                0x80 => InboundPacketType.TradeCancel,
                0x82 => InboundPacketType.ItemUse,
                0x83 => InboundPacketType.ItemUseOn,
                0x84 => InboundPacketType.ItemUseThroughBattleWindow,
                0x85 => InboundPacketType.ItemRotate,
                0x87 => InboundPacketType.ContainerClose,
                0x88 => InboundPacketType.ContainerUp,
                0x89 => InboundPacketType.WindowText,
                0x8A => InboundPacketType.WindowHouse,
                0x8C => InboundPacketType.LookAt,
                0x8D => InboundPacketType.LookThroughBattleWindow,
                0x96 => InboundPacketType.Speech,
                0x97 => InboundPacketType.ChannelListRequest,
                0x98 => InboundPacketType.ChannelOpen,
                0x99 => InboundPacketType.ChannelClose,
                0x9A => InboundPacketType.ChannelOpenDirect,
                0x9B => InboundPacketType.ReportStart,
                0x9C => InboundPacketType.ReportClose,
                0x9D => InboundPacketType.ReportCancel,
                0xA0 => InboundPacketType.ChangeModes,
                0xA1 => InboundPacketType.Attack,
                0xA2 => InboundPacketType.Follow,
                0xA3 => InboundPacketType.PartyInvite,
                0xA4 => InboundPacketType.PartyAcceptInvitation,
                0xA5 => InboundPacketType.PartyKick,
                0xA6 => InboundPacketType.PartyPassLeadership,
                0xA7 => InboundPacketType.PartyLeave,
                0xAA => InboundPacketType.ChannelCreatePrivate,
                0xAB => InboundPacketType.ChannelInvite,
                0xAC => InboundPacketType.ChannelExclude,
                0xBE => InboundPacketType.StopAllActions,
                0xC9 => InboundPacketType.ResendTile,
                0xCA => InboundPacketType.ResendContainer,
                0xCC => InboundPacketType.FindInContainer,
                0xD2 => InboundPacketType.StartOutfitChange,
                0xD3 => InboundPacketType.SubmitOutfitChange,
                0xDC => InboundPacketType.AddVip,
                0xDD => InboundPacketType.RemoveVip,
                0xE6 => InboundPacketType.ReportBug,
                0xE7 => InboundPacketType.ReportViolation,
                0xE8 => InboundPacketType.ReportDebugAssertion,
                _ => InboundPacketType.Unsupported,
            };
        }
    }
}
