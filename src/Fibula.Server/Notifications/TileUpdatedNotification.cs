// -----------------------------------------------------------------
// <copyright file="TileUpdatedNotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Notifications
{
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for a tile being updated.
    /// </summary>
    public class TileUpdatedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileUpdatedNotification"/> class.
        /// </summary>
        /// <param name="spectators">The spectators of this notification.</param>
        /// <param name="tile">The updated tile.</param>
        public TileUpdatedNotification(IEnumerable<IPlayer> spectators, ITile tile)
            : base(spectators)
        {
            this.Tile = tile;
        }

        /// <summary>
        /// Gets the updated tile.
        /// </summary>
        public ITile Tile { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new TileUpdatePacket(player, this.Tile).YieldSingleItem();
        }
    }
}
