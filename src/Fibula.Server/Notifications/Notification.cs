// -----------------------------------------------------------------
// <copyright file="Notification.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Abstract class that represents a notification to a player's connection.
    /// Notifications are basically any message that the server sends to the client of a specific player.
    /// </summary>
    public abstract class Notification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="targetPlayers">The set of players that should get this notification.</param>
        protected Notification(IEnumerable<IPlayer> targetPlayers)
        {
            this.TargetPlayers = targetPlayers;
        }

        /// <summary>
        /// Gets the target players for this notification.
        /// </summary>
        public IEnumerable<IPlayer> TargetPlayers { get; }

        /// <summary>
        /// Gets a value indicating whether this notification is final.
        /// </summary>
        public virtual bool IsFinal { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public abstract IEnumerable<IOutboundPacket> PrepareFor(IPlayer player);
    }
}
