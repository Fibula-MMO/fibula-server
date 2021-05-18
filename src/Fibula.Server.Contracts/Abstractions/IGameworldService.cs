// -----------------------------------------------------------------
// <copyright file="IGameworldService.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Entities;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Server.Contracts.Enumerations;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Interface for the game world service.
    /// </summary>
    public interface IGameworldService : IHostedService, IGameOperationsApi
    {
        /// <summary>
        /// Event fired when there is a notification ready to be broadcasted to the <see cref="IGameworldService"/>'s subscribers.
        /// </summary>
        event GameworldNotificationReadyHandler NotificationReady;

        /// <summary>
        /// Gets the bytes that represent the IPAddress at which this world is hosted.
        /// </summary>
        byte[] IpAddress { get; }

        /// <summary>
        /// Gets the port number at which this world is hosted.
        /// </summary>
        ushort Port { get; }

        /// <summary>
        /// Gets the state of the world.
        /// </summary>
        WorldState State { get; }

        /// <summary>
        /// Logs a player into the game.
        /// </summary>
        /// <param name="playerCreationMetadata">The metadata for the player's creation.</param>
        /// <returns>The id reserved for the player logging in.</returns>
        uint LogPlayerIn(CharacterEntity playerCreationMetadata);

        /// <summary>
        /// Logs a player out of the game.
        /// </summary>
        /// <param name="playerId">The id of the player to log out.</param>
        void LogPlayerOut(uint playerId);
    }
}
