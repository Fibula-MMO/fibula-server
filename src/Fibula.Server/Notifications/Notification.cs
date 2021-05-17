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
    using System;
    using System.Collections.Generic;
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
        /// <param name="findTargetPlayersFunc">A function to determine the target players of this notification.</param>
        protected Notification(Func<IEnumerable<IPlayer>> findTargetPlayersFunc)
        {
            this.FindTargetPlayers = findTargetPlayersFunc;
        }

        /// <summary>
        /// Gets the function for determining target players for this notification.
        /// </summary>
        public Func<IEnumerable<IPlayer>> FindTargetPlayers { get; }

        /// <summary>
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A <see cref="GameNotification"/> to be broadcasted.</returns>
        public abstract bool Post(INotificationContext context, IPlayer player);
    }
}
