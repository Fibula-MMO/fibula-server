// -----------------------------------------------------------------
// <copyright file="GameworldNotificationReadyHandler.cs" company="2Dudes">
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
    /// Delegate to handle when the <see cref="IGameworldService"/> has a new notification ready for its subscribers.
    /// </summary>
    /// <param name="service">The game world service that has the notification ready.</param>
    /// <param name="notification">The notification sent from the game server.</param>
    public delegate void GameworldNotificationReadyHandler(IGameworldService service, INotification notification);
}
