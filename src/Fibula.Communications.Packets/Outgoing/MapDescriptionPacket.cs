// -----------------------------------------------------------------
// <copyright file="MapDescriptionPacket.cs" company="2Dudes">
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
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a map description packet.
    /// </summary>
    public class MapDescriptionPacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapDescriptionPacket"/> class.
        /// </summary>
        /// <param name="player">The player that will be receiving the description.</param>
        /// <param name="origin">The origin location.</param>
        /// <param name="descriptionTiles">The description tiles.</param>
        public MapDescriptionPacket(IPlayer player, Location origin, IEnumerable<ITile> descriptionTiles)
        {
            this.Player = player;
            this.Origin = origin;
            this.DescriptionTiles = descriptionTiles;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType => OutboundPacketType.MapDescription;

        /// <summary>
        /// Gets the player that will be receiving the description.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Gets the origin location.
        /// </summary>
        public Location Origin { get; }

        /// <summary>
        /// Gets the description tiles.
        /// </summary>
        public IEnumerable<ITile> DescriptionTiles { get; }
    }
}
