// -----------------------------------------------------------------
// <copyright file="GameworldNotificationHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Contracts.Delegates
{
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate to handle a <see cref="IGameworldService"/>'s notification.
    /// </summary>
    /// <param name="notification">The notification sent from the game server.</param>
    public delegate void GameworldNotificationHandler(INotification notification);
}
