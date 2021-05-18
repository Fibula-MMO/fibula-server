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

namespace Fibula.Server.Contracts.Abstractions
{
    using System;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Interface for the API of game operations.
    /// </summary>
    public interface IGameOperationsApi
    {
        /// <summary>
        /// Cancels all operations that a player has pending, immediately.
        /// </summary>
        /// <param name="playerId">The id of the player to cancel operations for.</param>
        /// <param name="category">Optional. The specific category of operations to cancel. All operations are cancelled if no type is specified.</param>
        void CancelPlayerOperations(uint playerId, OperationCategory category = OperationCategory.Any);

        /// <summary>
        /// Cancels all operations that a player has pending asynchronously.
        /// </summary>
        /// <param name="playerId">The id of the player to cancel operations for.</param>
        /// <param name="category">Optional. The specific category of operations to cancel. All operations are cancelled if no type is specified.</param>
        void CancelPlayerOperationsAsync(uint playerId, OperationCategory category = OperationCategory.Any);

        /// <summary>
        /// Creates item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemType">The type of item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        /// <returns>True if the item is created successfully, false otherwise.</returns>
        bool CreateItemAtLocation(Location location, ItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes);

        /// <summary>
        /// Creates item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemType">The type of item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        void CreateItemAtLocationAsync(Location location, ItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes);

        /// <summary>
        /// Describes a thing for a player that is looking at it.
        /// </summary>
        /// <param name="thingId">The id of the thing to describe.</param>
        /// <param name="location">The location of the thing to describe.</param>
        /// <param name="stackPosition">The position in the stack within the location of the thing to describe.</param>
        /// <param name="playerId">The player for which to describe the thing for.</param>
        void DescribeThingAt(ushort thingId, Location location, byte stackPosition, uint playerId);

        /// <summary>
        /// Performs creature speech asynchronously.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The type of channel of the speech.</param>
        /// <param name="content">The content of the speech.</param>
        /// <param name="receiver">Optional. The receiver of the speech, if any.</param>
        void DoCreatureSpeechAsync(uint creatureId, SpeechType speechType, ChatChannelType channelType, string content, string receiver = "");

        /// <summary>
        /// Performs a creature turn asynchronously.
        /// </summary>
        /// <param name="requestorId">The id of the creature.</param>
        /// <param name="creatureId">The id of the creature to turn.</param>
        /// <param name="direction">The direction to turn to.</param>
        void DoCreatureTurnAsync(uint requestorId, uint creatureId, Direction direction);

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
        /// Places a new monster of the given race, at the given location.
        /// </summary>
        /// <param name="raceId">The id of race of monster to place.</param>
        /// <param name="location">The location at which to place the monster.</param>
        void PlaceNewMonsterAtAsync(string raceId, Location location);

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

        /// <summary>
        /// Sets the fight, chase and safety modes of a combatant.
        /// </summary>
        /// <param name="combatantId">The id of the combatant that updated modes.</param>
        /// <param name="fightMode">The fight mode to change to.</param>
        /// <param name="chaseMode">The chase mode to change to.</param>
        /// <param name="safeModeOn">A value indicating whether the attack safety lock is on.</param>
        void SetCombatantModes(uint combatantId, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn);

        /// <summary>
        /// Re-sets the attack target of the attacker and it's (possibly new) target.
        /// </summary>
        /// <param name="attackerId">The id of the attacker.</param>
        /// <param name="targetId">The id of the new target, which can be null.</param>
        void SetCombatantAttackTarget(uint attackerId, uint targetId);

        /// <summary>
        /// Re-sets the follow target of the combatant and it's (possibly new) target.
        /// </summary>
        /// <param name="followerId">The id of the follower.</param>
        /// <param name="targetId">The id of the new target, which can be null.</param>
        void SetCombatantFollowTarget(uint followerId, uint targetId);
    }
}
