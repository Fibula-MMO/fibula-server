// -----------------------------------------------------------------
// <copyright file="IClientsManager.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Contracts.Abstractions
{
    /// <summary>
    /// Interface for a manager of <see cref="IClient"/>s and their respective player ids.
    /// </summary>
    public interface IClientsManager
    {
        /// <summary>
        /// Registers a <see cref="IClient"/> in the map.
        /// </summary>
        /// <param name="client">The client to register.</param>
        void Register(IClient client);

        /// <summary>
        /// Unregisters a <see cref="IClient"/> in the map, given a player's id.
        /// </summary>
        /// <param name="playerId">The id of the player to unregister.</param>
        void Unregister(uint playerId);

        /// <summary>
        /// Attempts to find an <see cref="IClient"/> by a given player's id.
        /// </summary>
        /// <param name="playerId">The id to look for.</param>
        /// <returns>An instance of <see cref="IClient"/> if one was found, or null otherwise.</returns>
        IClient FindByPlayerId(uint playerId);
    }
}
