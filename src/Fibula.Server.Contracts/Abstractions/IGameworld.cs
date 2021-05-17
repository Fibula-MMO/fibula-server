// -----------------------------------------------------------------
// <copyright file="IGameworld.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Delegates;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Interface for the game service.
    /// </summary>
    public interface IGameworld : IHostedService, IGameOperationsApi, ICombatOperationsApi
    {
        /// <summary>
        /// Event fired when there is a notification ready to be broadcasted to the <see cref="IGameworld"/> world subscribers.
        /// </summary>
        event GameNotificationReadyHandler NotificationReady;

        /// <summary>
        /// Gets the game world's information.
        /// </summary>
        IWorldInformation WorldInfo { get; }

        // void OnPlayerInventoryChanged(IInventory inventory, Slot slot, IItem item);
    }
}
