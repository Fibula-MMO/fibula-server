// -----------------------------------------------------------------
// <copyright file="PlayerCancelAttackNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for cancelling a player's attack.
    /// </summary>
    public class PlayerCancelAttackNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCancelAttackNotification"/> class.
        /// </summary>
        /// <param name="player">The player for which the notification is for.</param>
        public PlayerCancelAttackNotification(IPlayer player)
            : base(player.YieldSingleItem())
        {
            player.ThrowIfNull(nameof(player));
        }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new PlayerCancelAttackPacket().YieldSingleItem();
        }
    }
}
