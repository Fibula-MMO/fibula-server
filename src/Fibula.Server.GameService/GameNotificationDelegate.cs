// -----------------------------------------------------------------
// <copyright file="GameNotificationDelegate.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.GameService
{
    /// <summary>
    /// A delegate that represents a notification from the game.
    /// </summary>
    /// <param name="worldId">The id of the world that this notification is about.</param>
    /// <param name="content">The content of the notification.</param>
    public delegate void GameNotificationDelegate(string worldId, string content);
}
