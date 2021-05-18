// -----------------------------------------------------------------
// <copyright file="TcpServerToGameworldAdapter.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Structures;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.TcpServer.Contracts.Delegates;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents an adapter between the tcp server instance and the game world server.
    /// This will be replaced by a full client-server framework (such as gRPC) in the future.
    /// </summary>
    public class TcpServerToGameworldAdapter : ITcpServerToGameworldAdapter
    {
        /// <summary>
        /// Stores a reference to the logger in use.
        /// </summary>
        private readonly ILogger<TcpServerToGameworldAdapter> logger;

        /// <summary>
        /// Stores a reference to the game instance.
        /// </summary>
        private readonly IGameworldService gameworld;

        /// <summary>
        /// Stores a reference to the application wide context.
        /// </summary>
        private readonly IApplicationContext applicationContext;

        /// <summary>
        /// The buffer for game notifications.
        /// </summary>
        private readonly BufferBlock<INotification> notificationsBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServerToGameworldAdapter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger to use.</param>
        /// <param name="applicationContext">A reference to the application context.</param>
        /// <param name="gameworldService">A reference to the game service.</param>
        public TcpServerToGameworldAdapter(ILogger<TcpServerToGameworldAdapter> logger, IApplicationContext applicationContext, IGameworldService gameworldService)
        {
            logger.ThrowIfNull(nameof(logger));
            applicationContext.ThrowIfNull(nameof(applicationContext));
            gameworldService.ThrowIfNull(nameof(gameworldService));

            this.logger = logger;
            this.applicationContext = applicationContext;
            this.gameworld = gameworldService;

            this.notificationsBuffer = new BufferBlock<INotification>();

            this.gameworld.NotificationReady += this.HandleGameNotification;
        }

        /// <summary>
        /// Attempts to log in to a given account, which includes the character list and some account details.
        /// </summary>
        /// <param name="accountIdentifier">The account identifier.</param>
        /// <param name="password">The password for this account.</param>
        /// <returns>A tuple composed of the collection of characters' information, or an error if there was one.</returns>
        public (IList<CharacterLoginInformation> characters, uint premDays, string error) GetAccountCharacterList(string accountIdentifier, string password)
        {
            using var unitOfWork = this.applicationContext.CreateNewUnitOfWork();

            var responseCharacters = new List<CharacterLoginInformation>();
            var responseError = string.Empty;
            uint responsePremiumDays = 0;

            // validate credentials.
            if (!uint.TryParse(accountIdentifier, out uint accountNumber) || !(unitOfWork.Accounts.FindByNumber(accountNumber) is AccountEntity account && account.Password.Equals(password)))
            {
                // TODO: hardcoded messages.
                responseError = "Please enter a valid account number and password.";
            }
            else if (account.Banished)
            {
                // Lift if time is up
                if (account.Banished && account.BanishedUntil > DateTimeOffset.UtcNow)
                {
                    // TODO: hardcoded messages.
                    responseError = "Your account is banished.";
                }
                else
                {
                    account.Banished = false;
                }
            }
            else if (account.Deleted)
            {
                // TODO: hardcoded messages.
                responseError = "Your account is disabled.\nPlease contact us for more information.";
            }
            else
            {
                // Try to add any characters found in the account.
                foreach (var character in account.Characters)
                {
                    responseCharacters.Add(new CharacterLoginInformation()
                    {
                        Name = character.Name,
                        Ip = this.gameworld.IpAddress,
                        Port = this.gameworld.Port,
                        World = character.World,
                    });
                }

                responsePremiumDays = (uint)(account.PremiumDays + account.TrialOrBonusPremiumDays);
            }

            // save any changes to the entities.
            unitOfWork.Complete();

            return (responseCharacters, responsePremiumDays, responseError);
        }

        /// <summary>
        /// Attempts to log in to a given account, which includes the character list and some account details.
        /// </summary>
        /// <param name="accountIdentifier">The account identifier.</param>
        /// <param name="password">The password for this account.</param>
        /// <param name="characterName">The name of the character to log into.</param>
        /// <returns>A tuple composed of the assigned player id, or an error if there was one.</returns>
        public (uint playerId, string error) RequestCharacterLogin(string accountIdentifier, string password, string characterName)
        {
            // TODO: hardcoded messages.
            uint responsePlayerId = 0;
            var responseError = string.Empty;

            using var unitOfWork = this.applicationContext.CreateNewUnitOfWork();

            if (!uint.TryParse(accountIdentifier, out uint accountNumber) || !(unitOfWork.Accounts.FindByNumber(accountNumber) is AccountEntity account && account.Password.Equals(password)))
            {
                responseError = "The account number and password combination is invalid.";
            }
            else if (account.Banished)
            {
                // Lift if time is up
                if (account.Banished && account.BanishedUntil > DateTimeOffset.UtcNow)
                {
                    responseError = "Your account is banished.";
                }
                else
                {
                    account.Banished = false;
                }
            }
            else if (account.Deleted)
            {
                responseError = "Your account is disabled.\nPlease contact us for more information.";
            }
            else if (!(unitOfWork.Characters.FindByName(characterName) is CharacterEntity character) || !character.AccountId.Equals(account.Id))
            {
                responseError = "The character selected was not found in this account.";
            }
            else if (account.Characters.FirstOrDefault(c => c.IsOnline && !c.Name.Equals(characterName)) is CharacterEntity otherCharacterOnline)
            {
                responseError = "Another character in your account is currently online.";
            }
            else if (this.gameworld.State == WorldState.Loading)
            {
                responseError = "The game is just starting.\nPlease try again in a few minutes.";
            }
            else if (this.gameworld.State == WorldState.Closed)
            {
                // Check if game is open to the public.
                responseError = "This game world is not open to the public yet.\nCheck your access or the news on our webpage.";
            }
            else
            {
                // Set player status to online.
                character.IsOnline = true;

                responsePlayerId = this.gameworld.LogPlayerIn(character);
            }

            // save any changes to the entities.
            unitOfWork.Complete();

            return (responsePlayerId, responseError);
        }

        /// <summary>
        /// Requests the gameworld to cancel a given player's pending operations.
        /// </summary>
        /// <param name="playerId">The id of the player for which to cancel operations.</param>
        /// <param name="category">Optional. The category of operations to cancel. Defaults to <see cref="OperationCategory.Any"/>.</param>
        public void RequestToCancelPlayerOperationsAsync(uint playerId, OperationCategory category = OperationCategory.Any)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests the game world to log out a player.
        /// </summary>
        /// <param name="playerId">The id of the player to try to log out.</param>
        public void RequestPlayerLogOutAsync(uint playerId)
        {
            this.gameworld.LogPlayerOut(playerId);
        }

        /// <summary>
        /// Requests the gameworld to set the creature as following a target.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="targetId">Optional. The id of the target, where zero means target or in other words, stop following.</param>
        public void RequestToFollowCreatureAsync(uint creatureId, uint targetId = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests the gameworld to set the creature as attacking a target.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="targetId">Optional. The id of the target, where zero means target or in other words, stop attacking.</param>
        public void RequestToAttackCreatureAsync(uint creatureId, uint targetId = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request the gameworld to turn a creature to a given direction.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="direction">The direction to turn it to.</param>
        public void RequestToTurnCreatureAsync(uint creatureId, Direction direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests the gameworld to set the player's fight, chase and safety modes to the given values.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="fightMode">The value to set as the fight mode.</param>
        /// <param name="chaseMode">The value to set as the chase mode.</param>
        /// <param name="safeModeOn">A value inidicating whether the safety lock should be set (prevents attacking others).</param>
        public void RequestToUpdateModesAsync(uint playerId, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request the gameworld to set a specific walk plan for the creature.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="directions">The specific directions to set as the creature's walk plan.</param>
        public void RequestToUpdateWalkPlanAsync(uint creatureId, Direction[] directions)
        {
            this.gameworld.ResetCreatureWalkPlan(creatureId, directions);
        }

        /// <summary>
        /// Request the gameworld to get the description of the top thing at a given location.
        /// </summary>
        /// <param name="playerId">The player for which to describe the thing.</param>
        /// <param name="location">The location at which to describe at.</param>
        /// <param name="indexHint">The index of the thing as observed by the client. The server treats this as a hint.</param>
        /// <param name="idHint">The id of the thing as observed by the client. The server treats this as a hint.</param>
        public void RequestTextDescriptionAtAsync(uint playerId, Location location, byte indexHint, ushort idHint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request sending a chat message on behalf of a player to a chat channel or another player.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The game channel over which the message is going.</param>
        /// <param name="content">The content of the message.</param>
        /// <param name="receiverId">Optional. The intended receiver of the message.</param>
        public void RequestSendMessageAsync(uint playerId, SpeechType speechType, ChatChannelType channelType, string content, string receiverId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subscribes the tcp server to the gameworld service, in order to begin receiving notifications.
        /// </summary>
        /// <param name="id">The id of the tcp server.</param>
        /// <param name="worldId">The id of the world to subscribe in.</param>
        /// <param name="notificationHandler">A handler to callback for when a notification from the gameworld is ready.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Subscribe(string id, string worldId, GameworldNotificationHandler notificationHandler, CancellationToken? cancellationToken = null)
        {
            notificationHandler.ThrowIfNull(nameof(notificationHandler));

            if (cancellationToken == null)
            {
                cancellationToken = CancellationToken.None;
            }

            while (!cancellationToken.Value.IsCancellationRequested)
            {
                // BufferBlock will wait for new data automagically.
                var notification = await this.notificationsBuffer.ReceiveAsync(cancellationToken.Value);

                try
                {
                    notificationHandler(notification);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Unsubscribes from the gameworld service.
        /// </summary>
        /// <param name="id">The id of the tcp server.</param>
        public void Unsubscribe(string id)
        {
            // Nothing to do in this adapter.
        }

        /// <summary>
        /// Handles a notification from the game world service.
        /// </summary>
        /// <param name="service">The game world service that has the notification ready.</param>
        /// <param name="notification">The notification sent from the game world.</param>
        private void HandleGameNotification(IGameworldService service, INotification notification)
        {
            if (notification == null)
            {
                return;
            }

            this.notificationsBuffer.Post(notification);
        }
    }
}
