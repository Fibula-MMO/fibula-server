// -----------------------------------------------------------------
// <copyright file="IGameOperationsApi.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Abstractions
{
    using Fibula.Definitions.Data.Structures;

    /// <summary>
    /// Interface for the API of game operations.
    /// </summary>
    public interface IGameOperationsApi
    {
        /// <summary>
        /// Attempts to place a creature at a given location on the map.
        /// </summary>
        /// <param name="targetLocation">The location to place the creature at.</param>
        /// <param name="creature">The creature to place.</param>
        /// <returns>True if the creature is successfully placed, false otherwise.</returns>
        bool AddCreatureToGame(Location targetLocation, ICreature creature);

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="notification">The notification to send.</param>
        void SendNotification(INotification notification);
    }
}
