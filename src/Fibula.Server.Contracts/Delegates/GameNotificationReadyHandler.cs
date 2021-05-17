// -----------------------------------------------------------------
// <copyright file="GameNotificationReadyHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Delegates
{
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate for an event fired when the <see cref="IGameworld"/> world has a new notification ready for its subscribers.
    /// </summary>
    /// <param name="notification">The notification sent from the game server.</param>
    public delegate void GameNotificationReadyHandler(INotification notification);
}
