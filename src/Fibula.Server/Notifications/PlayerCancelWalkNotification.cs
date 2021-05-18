// -----------------------------------------------------------------
// <copyright file="PlayerCancelWalkNotification.cs" company="2Dudes">
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
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for cancelling a player's walk.
    /// </summary>
    public class PlayerCancelWalkNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCancelWalkNotification"/> class.
        /// </summary>
        /// <param name="player">The player for which the walk cancellation happened.</param>
        public PlayerCancelWalkNotification(IPlayer player)
            : base(player.YieldSingleItem())
        {
            player.ThrowIfNull(nameof(player));

            this.Player = player;
        }

        /// <summary>
        /// Gets the player for which this announcement is for.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new PlayerCancelWalkPacket(player.Direction).YieldSingleItem();
        }
    }
}
