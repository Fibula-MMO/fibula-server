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

namespace Fibula.Mechanics.Contracts.Abstractions
{
    using System;
    using Fibula.Client.Contracts.Abstractions;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Common.Contracts.Enumerations;
    using Fibula.Common.Contracts.Structs;
    using Fibula.Creatures.Contracts.Abstractions;
    using Fibula.Data.Entities.Contracts.Abstractions;
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Interface for the operations available in the game API.
    /// </summary>
    public interface IGameOperationsApi
    {
        // void ChangeItem(IThing thing, ushort toItemId, byte effect);

        // void ChangeItemAtLocation(Location location, ushort fromItemId, ushort toItemId, byte effect);

        // void ChangePlayerStartLocation(IPlayer player, Location newLocation);

        // bool CompareItemCountAt(Location location, FunctionComparisonType comparisonType, ushort value);

        // bool CompareItemAttribute(IThing thing, ItemAttribute attribute, FunctionComparisonType comparisonType, ushort value);

        // void Damage(IThing damagingThing, IThing damagedThing, byte damageSourceType, ushort damageValue);

        // void Delete(IThing thing);

        // void DeleteOnMap(Location location, ushort itemId);

        // void DisplayAnimatedEffectAt(Location location, byte effectByte);

        // void DisplayAnimatedText(Location location, string text, byte textType);

        // bool HasAccessFlag(IPlayer player, string rightStr);

        // bool HasFlag(IThing itemThing, string flagStr);

        // bool HasProfession(IThing thing, byte profesionId);

        // bool IsAllowedToLogOut(IPlayer player);

        // bool IsAtLocation(IThing thing, Location location);

        // bool IsCreature(IThing thing);

        // bool IsDressed(IThing thing);

        // bool IsHouse(IThing thing);

        // bool IsHouseOwner(IThing thing, IPlayer user);

        // bool IsObjectThere(Location location, ushort typeId);

        // bool IsPlayer(IThing thing);

        // bool IsRandomNumberUnder(byte value, int maxValue = 100);

        // bool IsSpecificItem(IThing thing, ushort typeId);

        // void MoveTo(IThing thingToMove, Location targetLocation);

        // void MoveTo(ushort itemId, Location fromLocation, Location toLocation);

        // void MoveTo(Location fromLocation, Location targetLocation, params ushort[] exceptTypeIds);

        // void TagThing(IPlayer player, string format, IThing targetThing);

        /// <summary>
        /// Attempts to add content to the first possible parent container that accepts it, on a chain of parent containers.
        /// </summary>
        /// <param name="thingContainer">The first thing container to add to.</param>
        /// <param name="remainder">The remainder content to add, which overflows to the next container in the chain.</param>
        /// <param name="addIndex">The index at which to attempt to add, only for the first attempted container.</param>
        /// <param name="includeTileAsFallback">Optional. A value for whether to include tiles in the fallback chain.</param>
        /// <param name="requestorCreature">Optional. The creature requesting the addition of content.</param>
        /// <returns>True if the content was successfully added, false otherwise.</returns>
        bool AddContentToContainerOrFallback(IThingContainer thingContainer, ref IThing remainder, byte addIndex = byte.MaxValue, bool includeTileAsFallback = true, ICreature requestorCreature = null);

        /// <summary>
        /// Adds or aggregates a condition to an afflicted thing.
        /// </summary>
        /// <param name="thing">The thing to check the conditions on.</param>
        /// <param name="condition">The condition to add or extend.</param>
        /// <param name="duration">The duration for the condition being added.</param>
        void AddOrAggregateCondition(IThing thing, ICondition condition, TimeSpan duration);

        /// <summary>
        /// Cancels all operations that a player has pending, immediately.
        /// </summary>
        /// <param name="player">The player to cancel operations for.</param>
        /// <param name="typeOfOperationToCancel">Optional. The specific type of operation to cancel. All operations are cancelled if no type is specified.</param>
        void CancelPlayerOperations(IPlayer player, Type typeOfOperationToCancel = null);

        /// <summary>
        /// Cancels all operations that a player has pending asynchronously.
        /// </summary>
        /// <param name="player">The player to cancel operations for.</param>
        /// <param name="typeOfOperationToCancel">Optional. The specific type of operation to cancel. All operations are cancelled if no type is specified.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        IOperation CancelPlayerOperationsAsync(IPlayer player, Type typeOfOperationToCancel = null);

        /// <summary>
        /// Creates item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemType">The type of item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        /// <returns>True if the item is created successfully, false otherwise.</returns>
        bool CreateItemAtLocation(Location location, IItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes);

        /// <summary>
        /// Creates item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemType">The type of item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        IOperation CreateItemAtLocationAsync(Location location, IItemTypeEntity itemType, params (ItemAttribute, IConvertible)[] additionalAttributes);

        /// <summary>
        /// Creates a new item at the specified location.
        /// </summary>
        /// <param name="location">The location at which to create the item.</param>
        /// <param name="itemTypeId">The type id of the item to create.</param>
        /// <param name="additionalAttributes">Optional. Additional item attributes to set on the new item.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        IOperation CreateItemAtLocationAsync(Location location, ushort itemTypeId, params (ItemAttribute, IConvertible)[] additionalAttributes);

        /// <summary>
        /// Performs creature speech asynchronously.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The type of channel of the speech.</param>
        /// <param name="content">The content of the speech.</param>
        /// <param name="receiver">Optional. The receiver of the speech, if any.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        IOperation DoCreatureSpeechAsync(uint creatureId, SpeechType speechType, ChatChannelType channelType, string content, string receiver = "");

        /// <summary>
        /// Performs a creature turn asynchronously.
        /// </summary>
        /// <param name="requestorId">The id of the creature.</param>
        /// <param name="creature">The creature to turn.</param>
        /// <param name="direction">The direction to turn to.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        IOperation DoCreatureTurnAsync(uint requestorId, ICreature creature, Direction direction);

        /// <summary>
        /// Describes a thing for a player.
        /// </summary>
        /// <param name="thingId">The id of the thing to describe.</param>
        /// <param name="location">The location of the thing to describe.</param>
        /// <param name="stackPosition">The position in the stack within the location of the thing to describe.</param>
        /// <param name="player">The player for which to describe the thing for.</param>
        void LookAt(ushort thingId, Location location, byte stackPosition, IPlayer player);

        /// <summary>
        /// Logs a player into the game.
        /// </summary>
        /// <param name="client">The client from which the player is connecting.</param>
        /// <param name="creatureCreationMetadata">The metadata for the player's creation.</param>
        void LogPlayerIn(IClient client, ICreatureEntity creatureCreationMetadata);

        /// <summary>
        /// Logs a player out of the game.
        /// </summary>
        /// <param name="player">The player to log out.</param>
        void LogPlayerOut(IPlayer player);

        /// <summary>
        /// Moves a thing.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the move.</param>
        /// <param name="clientThingId">The id of the thing being moved.</param>
        /// <param name="fromLocation">The location from which the thing is being moved.</param>
        /// <param name="fromIndex">The index within the location from which the thing is being moved.</param>
        /// <param name="fromCreatureId">The id of the creature from which the thing is being moved, if any.</param>
        /// <param name="toLocation">The location to which the thing is being moved.</param>
        /// <param name="toCreatureId">The id of the creature to which the thing is being moved.</param>
        /// <param name="amount">Optional. The amount of the thing to move. Defaults to 1.</param>
        void Movement(uint requestorId, ushort clientThingId, Location fromLocation, byte fromIndex, uint fromCreatureId, Location toLocation, uint toCreatureId, byte amount = 1);

        /// <summary>
        /// Attempts to place a creature at a given location on the map.
        /// </summary>
        /// <param name="targetLocation">The location to place the creature at.</param>
        /// <param name="creature">The creature to place.</param>
        /// <returns>True if the creature is successfully placed, false otherwise.</returns>
        bool AddCreatureToGame(Location targetLocation, ICreature creature);

        /// <summary>
        /// Places a new monster of the given race, at the given location.
        /// </summary>
        /// <param name="raceId">The id of race of monster to place.</param>
        /// <param name="location">The location at which to place the monster.</param>
        /// <returns>The operation that was scheduled as a result, or null if nothing is done.</returns>
        IOperation PlaceNewMonsterAtAsync(string raceId, Location location);

        /// <summary>
        /// Attempts to remove a creature from the game.
        /// </summary>
        /// <param name="creature">The creature to remove.</param>
        /// <returns>True if the creature is successfully removed from the game, false otherwise.</returns>
        bool RemoveCreatureFromGame(ICreature creature);

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creature">The creature to reset the walk plan of.</param>
        /// <param name="directions">The directions for the new plan.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        /// <remarks>The walk plan is static for this overload.</remarks>
        void ResetCreatureWalkPlan(ICreature creature, Direction[] directions, WalkPlanStrategy strategy = WalkPlanStrategy.DoNotRecalculate);

        /// <summary>
        /// Resets a given creature's walk plan and kicks it off.
        /// </summary>
        /// <param name="creature">The creature to reset the walk plan of.</param>
        /// <param name="targetCreature">The creature towards which the walk plan will be generated to.</param>
        /// <param name="strategy">Optional. The strategy to follow in the plan.</param>
        /// <param name="targetDistance">Optional. The target distance to calculate from the target creature.</param>
        /// <param name="excludeCurrentPosition">Optional. A value indicating whether to exclude the current creature's position from being the goal location.</param>
        /// <remarks>The walk plan is dynamic for this overload.</remarks>
        void ResetCreatureWalkPlan(ICreature creature, ICreature targetCreature, WalkPlanStrategy strategy = WalkPlanStrategy.ConservativeRecalculation, int targetDistance = 1, bool excludeCurrentPosition = false);

        /// <summary>
        /// Sends a heartbeat to the player's client.
        /// </summary>
        /// <param name="player">The player which to send the heartbeat to.</param>
        /// <returns>The notification that was scheduled, or null if nothing happened.</returns>
        INotification SendHeartbeatAsync(IPlayer player);

        /// <summary>
        /// Sends a heartbeat response to the player's client.
        /// </summary>
        /// <param name="player">The player which to send the heartbeat response to.</param>
        /// <returns>The notification that was scheduled, or null if nothing happened.</returns>
        INotification SendHeartbeatResponseAsync(IPlayer player);
    }
}
