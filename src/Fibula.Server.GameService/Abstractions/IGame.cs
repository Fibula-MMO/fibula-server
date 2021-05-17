// -----------------------------------------------------------------
// <copyright file="IGame.cs" company="2Dudes">
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
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Interface for the game world instance.
    /// </summary>
    public interface IGame : IHostedService
    {
        /// <summary>
        /// Event fired when the game has a new notification.
        /// </summary>
        event GameNotificationDelegate NewNotification;
    }
}
