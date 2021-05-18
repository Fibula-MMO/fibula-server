// -----------------------------------------------------------------
// <copyright file="TileUpdatePacket.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Packets.Outgoing
{
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a tile update packet.
    /// </summary>
    public class TileUpdatePacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileUpdatePacket"/> class.
        /// </summary>
        /// <param name="player">The player that will be receiving the updated tile description.</param>
        /// <param name="tile">The tile that was updated.</param>
        public TileUpdatePacket(IPlayer player, ITile tile)
        {
            this.Player = player;
            this.UpdatedTile = tile;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType => OutboundPacketType.TileUpdate;

        /// <summary>
        /// Gets the player that will be receiving the description.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Gets the tile that was updated.
        /// </summary>
        public ITile UpdatedTile { get; }
    }
}
