// -----------------------------------------------------------------
// <copyright file="GameMock.cs" company="2Dudes">
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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a mock for a game instance, for PoC/testing purposes.
    /// </summary>
    public class GameMock : IGame
    {
        /// <summary>
        /// Stores a reference to the logger in use.
        /// </summary>
        private readonly ILogger<GameMock> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameMock"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public GameMock(ILogger<GameMock> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Event fired when the game has a new notification.
        /// </summary>
        public event GameNotificationDelegate NewNotification;

        /// <summary>
        /// Runs the main game processing thread which begins advancing time on the game engine.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(
                async () =>
                {
                    var randomTask = Task.Factory.StartNew(this.SendRandomNotifications, cancellationToken, TaskCreationOptions.LongRunning);

                    await Task.WhenAll(randomTask);
                },
                cancellationToken);

            // return this to allow other IHostedService-s to start.
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogWarning($"Cancellation requested on game instance, beginning shut-down...");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs continously and sends random notifications to simulate activity.
        /// </summary>
        /// <param name="tokenState">The state object which gets casted into a <see cref="CancellationToken"/>.</param>.
        private void SendRandomNotifications(object tokenState)
        {
            const int maximumSecondsToSleep = 2;

            var i = 0;
            var cancellationToken = (tokenState as CancellationToken?).Value;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Thread.Sleep is OK here because this runs on it's own thread.
                Thread.Sleep(TimeSpan.FromSeconds(new Random().NextDouble() * maximumSecondsToSleep));

                this.NewNotification?.Invoke("Fibula", $"Notification {i++}.");
            }
        }
    }
}
