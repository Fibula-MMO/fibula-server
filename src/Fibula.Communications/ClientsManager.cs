// -----------------------------------------------------------------
// <copyright file="ClientsManager.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a manager for <see cref="IClient"/>s.
    /// </summary>
    public class ClientsManager : IClientsManager
    {
        /// <summary>
        /// The mapping between each player id to the client it is connected to it.
        /// </summary>
        private readonly IDictionary<uint, IClient> playerToClientMap;

        /// <summary>
        /// An object to semaphore access to the <see cref="playerToClientMap"/>.
        /// </summary>
        private readonly object playerToClientMapLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsManager"/> class.
        /// </summary>
        public ClientsManager()
        {
            this.playerToClientMap = new Dictionary<uint, IClient>();
            this.playerToClientMapLock = new object();
        }

        /// <summary>
        /// Attempts to find an <see cref="IClient"/> by a given player's id.
        /// </summary>
        /// <param name="playerId">The id to look for.</param>
        /// <returns>An instance of <see cref="IClient"/> if one was found, or null otherwise.</returns>
        public IClient FindByPlayerId(uint playerId)
        {
            lock (this.playerToClientMapLock)
            {
                if (this.playerToClientMap.TryGetValue(playerId, out IClient client))
                {
                    return client;
                }
            }

            return null;
        }

        /// <summary>
        /// Registers a <see cref="IClient"/> in the map.
        /// </summary>
        /// <param name="client">The client to register.</param>
        public void Register(IClient client)
        {
            lock (this.playerToClientMapLock)
            {
                this.playerToClientMap[client.PlayerId] = client;
            }
        }

        /// <summary>
        /// Unregisters a <see cref="IClient"/> in the map, given a player's id.
        /// </summary>
        /// <param name="playerId">The id of the player to unregister.</param>
        public void Unregister(uint playerId)
        {
            lock (this.playerToClientMapLock)
            {
                this.playerToClientMap.Remove(playerId);
            }
        }
    }
}
