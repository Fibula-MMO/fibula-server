// -----------------------------------------------------------------
// <copyright file="ITcpServer.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Contracts.Abstractions
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Structures;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Interface for all TCP server instances.
    /// </summary>
    public interface ITcpServer : IHostedService
    {
        /// <summary>
        /// Gets the options for the server instance.
        /// </summary>
        ITcpServerOptions Options { get; }

        /// <summary>
        /// Gets the pre-login informational message, if any.
        /// </summary>
        string InformationalMessage { get; }

        /// <summary>
        /// Attempts to log in to a given account, which includes the character list and some account details.
        /// </summary>
        /// <param name="accountIdentifier">The account identifier.</param>
        /// <param name="password">The password for this account.</param>
        /// <returns>A tuple composed of the collection of characters' information, or an error if there was one.</returns>
        (IList<CharacterLoginInformation> characters, uint premDays, string error) DoAccountLogin(string accountIdentifier, string password);

        /// <summary>
        /// Requests the game world to log a player in.
        /// </summary>
        /// <param name="accountIdentifier">The character's account identifier.</param>
        /// <param name="password">The password for this character's account.</param>
        /// <param name="characterName">The name of the character to log into.</param>
        /// <returns>A tuple composed of the assigned player id, or an error if there was one.</returns>
        (uint playerId, string error) RequestPlayerLogIn(string accountIdentifier, string password, string characterName);

        /// <summary>
        /// Requests the game world to log out a player.
        /// </summary>
        /// <param name="playerId">The id of the player to try to log out.</param>
        void RequestPlayerLogOutAsync(uint playerId);

        /// <summary>
        /// Requests the gameworld to cancel a given player's pending operations.
        /// </summary>
        /// <param name="playerId">The id of the player for which to cancel operations.</param>
        /// <param name="category">Optional. The category of operations to cancel. Defaults to <see cref="OperationCategory.Any"/>.</param>
        void RequestToCancelPlayerOperationsAsync(uint playerId, OperationCategory category = OperationCategory.Any);

        /// <summary>
        /// Requests the gameworld to set the creature as following a target.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="targetId">Optional. The id of the target, where zero means target or in other words, stop following.</param>
        void RequestToFollowCreatureAsync(uint creatureId, uint targetId = 0);

        /// <summary>
        /// Requests the gameworld to set the creature as attacking a target.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="targetId">Optional. The id of the target, where zero means target or in other words, stop attacking.</param>
        void RequestToAttackCreatureAsync(uint creatureId, uint targetId = 0);

        /// <summary>
        /// Request the gameworld to turn a creature to a given direction.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="direction">The direction to turn it to.</param>
        void RequestToTurnCreatureAsync(uint creatureId, Direction direction);

        /// <summary>
        /// Requests the gameworld to set the player's fight, chase and safety modes to the given values.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="fightMode">The value to set as the fight mode.</param>
        /// <param name="chaseMode">The value to set as the chase mode.</param>
        /// <param name="safeModeOn">A value inidicating whether the safety lock should be set (prevents attacking others).</param>
        void RequestToUpdateModesAsync(uint playerId, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn);

        /// <summary>
        /// Request the gameworld to set a specific walk plan for the creature.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="directions">The specific directions to set as the creature's walk plan.</param>
        void RequestToUpdateWalkPlanAsync(uint creatureId, Direction[] directions);

        /// <summary>
        /// Request the gameworld to get the description of the top thing at a given location.
        /// </summary>
        /// <param name="playerId">The player for which to describe the thing.</param>
        /// <param name="location">The location at which to describe at.</param>
        /// <param name="indexHint">The index of the thing as observed by the client. The server treats this as a hint.</param>
        /// <param name="idHint">The id of the thing as observed by the client. The server treats this as a hint.</param>
        void RequestTextDescriptionAtAsync(uint playerId, Location location, byte indexHint, ushort idHint);

        /// <summary>
        /// Request sending a chat message on behalf of a player to a chat channel or another player.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The game channel over which the message is going.</param>
        /// <param name="content">The content of the message.</param>
        /// <param name="receiverId">Optional. The intended receiver of the message.</param>
        void RequestSendMessageAsync(uint playerId, SpeechType speechType, ChatChannelType channelType, string content, string receiverId);

        /// <summary>
        /// Sends a heartbeat packet to a client.
        /// </summary>
        /// <param name="client">The client which to send the heartbeat to.</param>
        void SendHeartbeatAsync(IClient client);

        /// <summary>
        /// Sends a heartbeat response packet to a client.
        /// </summary>
        /// <param name="client">The client which to send the heartbeat response to.</param>
        void SendHeartbeatResponseAsync(IClient client);
    }
}
