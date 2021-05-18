// -----------------------------------------------------------------
// <copyright file="TileUpdatePacketWriter.cs" company="2Dudes">
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
    using System.Buffers;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Protocol.Contracts;
    using Fibula.Protocol.Contracts.Abstractions;
    using Fibula.Protocol.V772.Extensions;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a tile update packet writer for the game server.
    /// </summary>
    public class TileUpdatePacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Stores the reference to the tile descriptor in use.
        /// </summary>
        private readonly ITileDescriptor tileDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileUpdatePacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="tileDescriptor">A reference to the tile descriptor in use.</param>
        public TileUpdatePacketWriter(ILogger<TileUpdatePacketWriter> logger, ITileDescriptor tileDescriptor)
            : base(logger)
        {
            this.tileDescriptor = tileDescriptor;
        }

        /// <summary>
        /// Writes a packet to the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="packet">The packet to write.</param>
        /// <param name="message">The message to write into.</param>
        public override void WriteToMessage(IOutboundPacket packet, ref INetworkMessage message)
        {
            if (!(packet is TileUpdatePacket tileUpdatePacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            message.AddByte(tileUpdatePacket.PacketType.ToByte());

            message.AddLocation(tileUpdatePacket.UpdatedTile.Location);

            if (tileUpdatePacket.UpdatedTile != null)
            {
                message.AddBytes(this.BuildDescription(tileUpdatePacket.Player, tileUpdatePacket.UpdatedTile));
                message.AddByte(0x00); // skip count
            }
            else
            {
                message.AddByte(0x01); // skip count
            }

            // marks the end.
            message.AddByte(byte.MaxValue);
        }

        private ReadOnlySequence<byte> BuildDescription(IPlayer player, ITile tile)
        {
            byte currentSkipCount = byte.MaxValue;
            var firstSegment = new BytesSegment(ReadOnlyMemory<byte>.Empty);

            BytesSegment lastSegment = firstSegment;

            var segmentsFromTile = this.tileDescriptor.DescribeTileForPlayer(player, tile);

            // See if we actually have segments to append.
            if (segmentsFromTile != null && segmentsFromTile.Any())
            {
                if (currentSkipCount < byte.MaxValue)
                {
                    lastSegment = lastSegment.Append(new byte[] { currentSkipCount, byte.MaxValue });
                }

                foreach (var segment in segmentsFromTile)
                {
                    lastSegment.Append(segment);
                    lastSegment = segment;
                }

                currentSkipCount = byte.MinValue;
            }

            if (++currentSkipCount == byte.MaxValue)
            {
                lastSegment = lastSegment.Append(new byte[] { byte.MaxValue, byte.MaxValue });
            }

            if (++currentSkipCount < byte.MaxValue)
            {
                lastSegment = lastSegment.Append(new byte[] { currentSkipCount, byte.MaxValue });
            }

            return new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
        }
    }
}
