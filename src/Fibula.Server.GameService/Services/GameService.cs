// -----------------------------------------------------------------
// <copyright file="GameService.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.GameService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Services.Game;
    using Grpc.Core;
    using Microsoft.Extensions.Logging;

    using static Fibula.Services.Game.Gameworld;

    /// <summary>
    /// Class that represents the Game world service.
    /// </summary>
    public class GameService : GameworldBase
    {
        /// <summary>
        /// An object used as a locking mechanism for the <see cref="Subscribers"/> set.
        /// </summary>
        private static readonly object SubscribersLock = new object();

        /// <summary>
        /// A map of categories to their subscribers.
        /// </summary>
        private static readonly ISet<string> Subscribers = new HashSet<string>();

        /// <summary>
        /// Stores a reference to the logger in use.
        /// </summary>
        private readonly ILogger<GameService> logger;

        /// <summary>
        /// Stores a reference to the game instance.
        /// </summary>
        private readonly IGame gameInstance;

        /// <summary>
        /// The buffer for game events.
        /// </summary>
        private readonly BufferBlock<GameEvent> buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger to use.</param>
        /// <param name="game">A reference to the game instance.</param>
        public GameService(ILogger<GameService> logger, IGame game)
        {
            this.logger = logger;
            this.gameInstance = game;

            this.buffer = new BufferBlock<GameEvent>();

            this.gameInstance.NewNotification += this.HandleGameNotification;
        }

        /// <summary>
        /// Handles a request from a client to subscribe to the Game world server's events.
        /// </summary>
        /// <param name="request">The subscription information.</param>
        /// <param name="eventsStream">The events stream which to pump game events back to the subscriber.</param>
        /// <param name="context">A context for this call.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task Subscribe(SubscriptionRequest request, IServerStreamWriter<GameEvent> eventsStream, ServerCallContext context)
        {
            this.logger.LogInformation($"Peer {request.SubscriberId} with IP {context.Peer} subscribed.");

            // Add to the subscribers list.
            lock (SubscribersLock)
            {
                Subscribers.Add(request.SubscriberId);
            }

            while (!context.CancellationToken.IsCancellationRequested)
            {
                // BufferBlock will wait for new data automagically.
                var gameEvent = await this.buffer.ReceiveAsync(context.CancellationToken);

                // Skip events from worlds we don't care about.
                if (!gameEvent.WorldId.Equals(request.WorldId))
                {
                    continue;
                }

                if (Subscribers.Contains(request.SubscriberId))
                {
                    // Write the event to the subscriber.
                    try
                    {
                        await eventsStream.WriteAsync(gameEvent);
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.logger.LogError(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Handles a request from a client to unsubscribe from the Game world server's events.
        /// </summary>
        /// <param name="request">The subscription information.</param>
        /// <param name="context">A context for this call.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task<UnsubscriptionResponse> Unsubscribe(UnsubscriptionRequest request, ServerCallContext context)
        {
            lock (SubscribersLock)
            {
                var removed = Subscribers.Remove(request.SubscriberId);

                return Task.FromResult(new UnsubscriptionResponse() { Message = removed ? "You unsubscribed from game events." : "You were not in the subscribers list anyways..." });
            }
        }

        /// <summary>
        /// Handles a new notification from the game instance.
        /// </summary>
        /// <param name="worldId">The id of the world that this notification is about.</param>
        /// <param name="content">The content of the notification.</param>
        private void HandleGameNotification(string worldId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            var evt = this.GenerateRandomEvent(worldId);

            this.buffer.Post(evt);

            this.logger.LogInformation($"Posted a new game event from world {worldId} into the buffer.");
        }

        private GameEvent GenerateRandomEvent(string worldId)
        {
            Random rng = new Random();

            if (rng.Next() % 2 == 0)
            {
                return new GameEvent()
                {
                    WorldId = worldId,
                    Login = new PlayerLogin()
                    {
                        Name = "some player",
                        SomethingElse = "wuuu",
                    },
                };
            }

            return new GameEvent()
            {
                WorldId = worldId,
                Effect = new MaggicEffect()
                {
                    Type = "sometype",
                    Location = "1,2,3",
                },
            };
        }
    }
}
