// -----------------------------------------------------------------
// <copyright file="INotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Abstractions
{
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;

    /// <summary>
    /// Interface for all notifications.
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// Gets the target players for this notification.
        /// </summary>
        IEnumerable<IPlayer> TargetPlayers { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        IEnumerable<IOutboundPacket> PrepareFor(IPlayer player);
    }
}
