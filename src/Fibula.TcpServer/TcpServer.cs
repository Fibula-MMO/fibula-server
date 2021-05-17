// -----------------------------------------------------------------
// <copyright file="TcpServer.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Structures;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class that represents an instance of the TCP Server.
    /// </summary>
    public class TcpServer : ITcpServer
    {
        /// <summary>
        /// Defines the <see cref="TimeSpan"/> to wait between checks for idle players and connections.
        /// </summary>
        private static readonly TimeSpan IdleCheckDelay = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Stores the id for this instance.
        /// </summary>
        private readonly string id;

        /// <summary>
        /// The logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The mapping between each connection to the client it represents.
        /// </summary>
        private readonly IDictionary<IConnection, IClient> connectionToClientMap;

        /// <summary>
        /// The manager for clients and their associated players.
        /// </summary>
        private readonly IClientsManager clientsManager;

        /// <summary>
        /// The adapter acting as a client to the gameworld instance that this server interacts with.
        /// </summary>
        private readonly ITcpServerToGameworldAdapter gameworldClientAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="options">The options to intialize the instance with.</param>
        /// <param name="loggerFactory">A reference to the factory of loggers to use.</param>
        /// <param name="handlerSelector">A refrence to the selector of handlers for TCP messages.</param>
        /// <param name="clientsManager">A reference to a manager for clients.</param>
        /// <param name="listeners">A reference to the listeners to bind to the server.</param>
        /// <param name="gameworldAdapter">A reference to an adapter that will act as a client to the game world.</param>
        public TcpServer(
            IOptions<TcpServerOptions> options,
            ILoggerFactory loggerFactory,
            IHandlerSelector handlerSelector,
            IClientsManager clientsManager,
            IEnumerable<IListener> listeners,
            ITcpServerToGameworldAdapter gameworldAdapter)
        {
            options.ThrowIfNull(nameof(options));
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            handlerSelector.ThrowIfNull(nameof(handlerSelector));
            listeners.ThrowIfNull(nameof(listeners));
            gameworldAdapter.ThrowIfNull(nameof(gameworldAdapter));

            if (!listeners.Any())
            {
                throw new ArgumentException("No listeners found after composition.", nameof(listeners));
            }

            DataAnnotationsValidator.ValidateObjectRecursive(options.Value);

            this.id = Guid.NewGuid().ToString();
            this.logger = loggerFactory.CreateLogger<TcpServer>();
            this.gameworldClientAdapter = gameworldAdapter;
            this.connectionToClientMap = new Dictionary<IConnection, IClient>();
            this.clientsManager = clientsManager;

            this.Options = options.Value;

            this.BindListeners(loggerFactory, handlerSelector, listeners);
        }

        /// <summary>
        /// Gets the options for this service instance.
        /// </summary>
        public ITcpServerOptions Options { get; private set; }

        /// <summary>
        /// Gets the any current informational message to send to players before loging in.
        /// </summary>
        public string InformationalMessage { get; }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(
                async () =>
                {
                    var connectionSweepperTask = Task.Factory.StartNew(this.IdlePlayerSweep, cancellationToken, TaskCreationOptions.LongRunning);

                    // Subscribe to the gameworld server, to tell it we are now available.
                    var subscriptionTask = this.gameworldClientAdapter.Subscribe(this.id, this.Options.WorldId, this.HandleGameNotification, cancellationToken);

                    await Task.WhenAll(connectionSweepperTask, subscriptionTask);

                    // Give notice to the gameworld server as well.
                    this.gameworldClientAdapter.Unsubscribe(this.id);
                },
                cancellationToken);

            // return this to allow other IHostedService-s to start.
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // We're spinning down, so best-effort let the gameserver know.
            this.gameworldClientAdapter.Unsubscribe(this.id);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Attempts to log in to a given account, which includes the character list and some account details.
        /// </summary>
        /// <param name="accountIdentifier">The account identifier.</param>
        /// <param name="password">The password for this account.</param>
        /// <returns>A tuple composed of the collection of characters' information, or an error if there was one.</returns>
        public (IList<CharacterLoginInformation> characters, uint premDays, string error) DoAccountLogin(string accountIdentifier, string password)
        {
            if (string.IsNullOrWhiteSpace(accountIdentifier) || string.IsNullOrWhiteSpace(password))
            {
                return (null, 0, "You must enter both an account number and password.");
            }

            var (characters, premiumDays, error) = this.gameworldClientAdapter.GetAccountCharacterList(accountIdentifier, password);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return (null, 0, error);
            }

            if (!characters.Any())
            {
                return (null, 0, $"You don't have any characters in your account.\nPlease create a new character in our web site: {this.Options.WebsiteUrl}");
            }

            return (characters, premiumDays, string.Empty);
        }

        /// <summary>
        /// Requests the game world to log a player in.
        /// </summary>
        /// <param name="accountIdentifier">The character's account identifier.</param>
        /// <param name="password">The password for this character's account.</param>
        /// <param name="characterName">The name of the character to log into.</param>
        /// <returns>A tuple composed of the assigned player id, or an error if there was one.</returns>
        public (uint playerId, string error) RequestPlayerLogIn(string accountIdentifier, string password, string characterName)
        {
            if (string.IsNullOrWhiteSpace(accountIdentifier) || string.IsNullOrWhiteSpace(password))
            {
                return (0, "You must enter both an account number and password.");
            }

            if (string.IsNullOrWhiteSpace(characterName))
            {
                return (0, "You must pick a character.");
            }

            var (playerId, error) = this.gameworldClientAdapter.RequestCharacterLogin(accountIdentifier, password, characterName);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return (0, error);
            }

            return (playerId, string.Empty);
        }

        /// <summary>
        /// Sends a heartbeat packet to a client.
        /// </summary>
        /// <param name="client">The client which to send the heartbeat to.</param>
        public void SendHeartbeatAsync(IClient client)
        {
            if (client == null)
            {
                return;
            }

            client.Send(new HeartbeatPacket().YieldSingleItem());
        }

        /// <summary>
        /// Sends a heartbeat response packet to a client.
        /// </summary>
        /// <param name="client">The client which to send the heartbeat response to.</param>
        public void SendHeartbeatResponseAsync(IClient client)
        {
            if (client == null)
            {
                return;
            }

            client.Send(new HeartbeatResponsePacket().YieldSingleItem());
        }

        /// <summary>
        /// Requests the gameworld to cancel a given player's pending operations.
        /// </summary>
        /// <param name="playerId">The id of the player for which to cancel operations.</param>
        /// <param name="category">Optional. The category of operations to cancel. Defaults to <see cref="OperationCategory.Any"/>.</param>
        public void RequestToCancelPlayerOperationsAsync(uint playerId, OperationCategory category = OperationCategory.Any)
        {
            this.gameworldClientAdapter.RequestToCancelPlayerOperationsAsync(playerId, category);
        }

        /// <summary>
        /// Requests the gameworld to log out a given player.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        public void RequestPlayerLogOutAsync(uint playerId)
        {
            this.gameworldClientAdapter.RequestPlayerLogOutAsync(playerId);
        }

        /// <summary>
        /// Requests the gameworld to set a creature as following a given target.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="targetId">Optional. The id of the target.</param>
        public void RequestToFollowCreatureAsync(uint creatureId, uint targetId = 0)
        {
            this.gameworldClientAdapter.RequestToFollowCreatureAsync(creatureId, targetId);
        }

        /// <summary>
        /// Requests the gameworld to set a creature as attacking a given target.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="targetId">Optional. The id of the target.</param>
        public void RequestToAttackCreatureAsync(uint creatureId, uint targetId = 0)
        {
            this.gameworldClientAdapter.RequestToAttackCreatureAsync(creatureId, targetId);
        }

        /// <summary>
        /// Requests the gameworld to turn a creature to a given direction.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="direction">The direction to turn to.</param>
        public void RequestToTurnCreatureAsync(uint creatureId, Direction direction)
        {
            this.gameworldClientAdapter.RequestToTurnCreatureAsync(creatureId, direction);
        }

        /// <summary>
        /// Requests the gameworld to update a player's modes.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="fightMode">The chosen fight mode.</param>
        /// <param name="chaseMode">The chosen chase mode.</param>
        /// <param name="safeModeOn">A value indicating whether the safety mode is on.</param>
        public void RequestToUpdateModesAsync(uint playerId, FightMode fightMode, ChaseMode chaseMode, bool safeModeOn)
        {
            this.gameworldClientAdapter.RequestToUpdateModesAsync(playerId, fightMode, chaseMode, safeModeOn);
        }

        /// <summary>
        /// Requests the gameworld to update a given creature's walk plan.
        /// </summary>
        /// <param name="creatureId">The id of the creature.</param>
        /// <param name="directions">The new directions of the walk plan.</param>
        public void RequestToUpdateWalkPlanAsync(uint creatureId, Direction[] directions)
        {
            this.gameworldClientAdapter.RequestToUpdateWalkPlanAsync(creatureId, directions);
        }

        /// <summary>
        /// Requests the gameworld to give a text description at a given location to the player.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="location">The location to describe.</param>
        /// <param name="indexHint">A hint to the game as to what index within the location the player is looking at.</param>
        /// <param name="idHint">A hint to the game as to what the player thinks it is looking at.</param>
        public void RequestTextDescriptionAtAsync(uint playerId, Location location, byte indexHint, ushort idHint)
        {
            this.gameworldClientAdapter.RequestTextDescriptionAtAsync(playerId, location, indexHint, idHint);
        }

        /// <summary>
        /// Requests the gameworld to send a message on behalf of a player.
        /// </summary>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="speechType">The type of speech this is.</param>
        /// <param name="channelType">The game channel over which the message should be sent.</param>
        /// <param name="content">The content of the message.</param>
        /// <param name="receiverId">The intended receiver of the message, if any.</param>
        public void RequestSendMessageAsync(uint playerId, SpeechType speechType, ChatChannelType channelType, string content, string receiverId)
        {
            this.gameworldClientAdapter.RequestSendMessageAsync(playerId, speechType, channelType, content, receiverId);
        }

        /// <summary>
        /// Binds the TCP listener events with handler picking.
        /// </summary>
        /// <param name="loggerFactory">The loggers factory to create loggers with.</param>
        /// <param name="handlerSelector">The selector of handlers to bind listeners' events to.</param>
        /// <param name="listeners">The listeners to bind.</param>
        private void BindListeners(ILoggerFactory loggerFactory, IHandlerSelector handlerSelector, IEnumerable<IListener> listeners)
        {
            IEnumerable<IOutboundPacket> OnPacketReady(IConnection connection, IInboundPacket packet)
            {
                var handler = handlerSelector.SelectForPacket(packet);

                if (handler != null && this.connectionToClientMap.TryGetValue(connection, out IClient client))
                {
                    return handler.HandleRequestPacket(this, packet, client);
                }

                return null;
            }

            void OnConnectionClosed(IConnection connection)
            {
                // Clean up the event listeners we set up here.
                connection.PacketReady -= OnPacketReady;
                connection.Closed -= OnConnectionClosed;

                if (!this.connectionToClientMap.ContainsKey(connection))
                {
                    return;
                }

                var playerId = this.connectionToClientMap[connection].PlayerId;

                this.clientsManager.Unregister(playerId);
                this.connectionToClientMap.Remove(connection);
            }

            void OnNewConnection(IConnection connection)
            {
                if (this.connectionToClientMap.ContainsKey(connection))
                {
                    return;
                }

                this.connectionToClientMap.Add(connection, new ConnectedClient(loggerFactory.CreateLogger<ConnectedClient>(), connection));

                connection.PacketReady += OnPacketReady;

                connection.Closed += OnConnectionClosed;
            }

            foreach (var tcpListener in listeners)
            {
                tcpListener.NewConnection += OnNewConnection;
            }
        }

        /// <summary>
        /// Cleans up players who's connections are orphaned, or (TODO) have been idle for some time.
        /// </summary>
        /// <param name="tokenState">The state object which gets casted into a <see cref="CancellationToken"/>.</param>.
        private void IdlePlayerSweep(object tokenState)
        {
            var cancellationToken = (tokenState as CancellationToken?).Value;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Thread.Sleep is OK here because this runs on it's own thread.
                Thread.Sleep(IdleCheckDelay);

                // Now let's clean up and try to log out all orphaned ones.
                foreach (var client in this.connectionToClientMap.Values)
                {
                    if (client != null && !client.IsIdle)
                    {
                        if (client.Type == AgentType.Linux || client.Type == AgentType.OtClientLinux || client.Type == AgentType.OtClientWindows)
                        {
                            this.SendHeartbeatAsync(client);
                        }

                        continue;
                    }

                    this.RequestPlayerLogOutAsync(client.PlayerId);
                }
            }
        }

        private void HandleGameNotification(INotification notification)
        {
            if (notification == null)
            {
                return;
            }

            try
            {
                if (notification.TargetPlayers == null || !notification.TargetPlayers.Any())
                {
                    // This one is verbose because it is really common, for example, expiring items or spawning creatures,
                    // which mostly tend to happen while there are no players around.
                    this.logger?.LogTrace($"{notification.GetType().Name} with no targets, skipping.");
                    return;
                }

                foreach (var player in notification.TargetPlayers)
                {
                    if (player == null || !(this.clientsManager.FindByPlayerId(player.Id) is IClient client))
                    {
                        continue;
                    }

                    INetworkMessage outboundMessage = new NetworkMessage();
                    IEnumerable<IOutboundPacket> outgoingPackets = notification.PrepareFor(player);

                    if (outgoingPackets == null || !outgoingPackets.Any())
                    {
                        continue;
                    }

                    client.Send(outgoingPackets);
                }
            }
            catch (Exception ex)
            {
                this.logger?.LogError($"Error while sending {notification.GetType().Name}: {ex.Message}");
            }
        }
    }
}
