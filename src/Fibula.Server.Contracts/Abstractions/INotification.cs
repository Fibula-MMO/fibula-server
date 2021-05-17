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

namespace Fibula.Server.Contracts.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for all notifications.
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// Gets the function for determining target players for this notification.
        /// </summary>
        Func<IEnumerable<IPlayer>> FindTargetPlayers { get; }

        /// <summary>
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>True if the notification was posted successfuly, and false otherwise.</returns>
        bool Post(INotificationContext context, IPlayer player);
    }
}
