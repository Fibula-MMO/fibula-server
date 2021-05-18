// -----------------------------------------------------------------
// <copyright file="DisconnectNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for a disconnection.
    /// </summary>
    public class DisconnectNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectNotification"/> class.
        /// </summary>
        /// <param name="targetPlayers">The target players of this notification.</param>
        /// <param name="reason">The reason for the disconnection.</param>
        public DisconnectNotification(IEnumerable<IPlayer> targetPlayers, string reason)
            : base(targetPlayers)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Gets the reason for the disconnection.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new GameServerDisconnectPacket(this.Reason).YieldSingleItem();
        }
    }
}
