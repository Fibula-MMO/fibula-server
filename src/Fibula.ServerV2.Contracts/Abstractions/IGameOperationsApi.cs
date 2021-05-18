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
    using Fibula.Definitions.Enumerations;

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
        /// Attempts to remove a creature from the game.
        /// </summary>
        /// <param name="creature">The creature to remove.</param>
        /// <returns>True if the creature is successfully removed from the game, false otherwise.</returns>
        bool RemoveCreatureFromGame(ICreature creature);

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creatureId">The id of the creature to reset the walk plan of.</param>
        /// <param name="directions">The directions for the new plan.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        void ResetCreatureWalkPlan(uint creatureId, Direction[] directions, WalkPlanStrategy strategy = WalkPlanStrategy.DoNotRecalculate);

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creatureId">The id of the creature to reset the walk plan of.</param>
        /// <param name="targetCreature">The creature towards which the walk plan will be generated to.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        /// <param name="targetDistance">Optional. The target distance to calculate from the target creature.</param>
        /// <param name="excludeCurrentLocation">Optional. A value indicating whether to exclude the current creature's location from being the goal location.</param>
        void ResetCreatureWalkPlan(uint creatureId, ICreature targetCreature, WalkPlanStrategy strategy = WalkPlanStrategy.ConservativeRecalculation, int targetDistance = 1, bool excludeCurrentLocation = false);

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="notification">The notification to send.</param>
        void SendNotification(INotification notification);
    }
}
