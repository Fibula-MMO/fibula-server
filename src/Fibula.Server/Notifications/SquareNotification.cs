// -----------------------------------------------------------------
// <copyright file="SquareNotification.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for a world light change.
    /// </summary>
    public class SquareNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquareNotification"/> class.
        /// </summary>
        /// <param name="player">The target player of this notification.</param>
        /// <param name="creatureId">The id of the creature over which to display the square.</param>
        /// <param name="color">The color of the square.</param>
        public SquareNotification(IPlayer player, uint creatureId, SquareColor color)
            : base(player.YieldSingleItem())
        {
            this.CreatureId = creatureId;
            this.Color = color;
        }

        /// <summary>
        /// Gets the id of the creature over which to display the square.
        /// </summary>
        public uint CreatureId { get; }

        /// <summary>
        /// Gets the color of the square.
        /// </summary>
        public SquareColor Color { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new SquarePacket(this.CreatureId, this.Color).YieldSingleItem();
        }
    }
}
