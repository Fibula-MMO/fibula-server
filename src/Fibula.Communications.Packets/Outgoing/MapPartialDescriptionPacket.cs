// -----------------------------------------------------------------
// <copyright file="MapPartialDescriptionPacket.cs" company="2Dudes">
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
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a partial map description packet.
    /// </summary>
    public class MapPartialDescriptionPacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPartialDescriptionPacket"/> class.
        /// </summary>
        /// <param name="mapDescriptionType">The type of map description.</param>
        /// <param name="player">The player that will be receiving the description.</param>
        /// <param name="descriptionTiles">The description tiles.</param>
        public MapPartialDescriptionPacket(OutboundPacketType mapDescriptionType, IPlayer player, IEnumerable<ITile> descriptionTiles)
        {
            if (mapDescriptionType != OutboundPacketType.MapSliceEast &&
                mapDescriptionType != OutboundPacketType.MapSliceNorth &&
                mapDescriptionType != OutboundPacketType.MapSliceSouth &&
                mapDescriptionType != OutboundPacketType.MapSliceWest &&
                mapDescriptionType != OutboundPacketType.FloorChangeUp &&
                mapDescriptionType != OutboundPacketType.FloorChangeDown)
            {
                throw new ArgumentException($"Unsupported partial description type {mapDescriptionType}.", nameof(mapDescriptionType));
            }

            this.PacketType = mapDescriptionType;
            this.Player = player;
            this.DescriptionTiles = descriptionTiles;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType { get; }

        /// <summary>
        /// Gets the player that will be receiving the description.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Gets the description tiles.
        /// </summary>
        public IEnumerable<ITile> DescriptionTiles { get; }
    }
}
