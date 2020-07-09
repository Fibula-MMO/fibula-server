﻿// -----------------------------------------------------------------
// <copyright file="BaseListener.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Listeners
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Common.Utilities;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Contracts.Delegates;
    using Fibula.Security.Contracts;
    using Serilog;

    /// <summary>
    /// Class that is the base implementation for all TCP listeners.
    /// </summary>
    public abstract class BaseListener : IListener
    {
        /// <summary>
        /// The TCP listener to use internally.
        /// </summary>
        private readonly ITcpListener tcpListener;

        /// <summary>
        /// The DoS defender to use.
        /// </summary>
        private readonly IDoSDefender dosDefender;

        /// <summary>
        /// A value indicating whether the protocol should keep the connection open after recieving a packet.
        /// </summary>
        private readonly bool keepConnectionOpen;

        /// <summary>
        /// A reference to the factory used to create socket connections based on TCP sockets.
        /// </summary>
        private readonly ISocketConnectionFactory socketConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseListener"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="options">The options for this listener.</param>
        /// <param name="socketConnectionFactory">A reference to the socekt connection factory in use.</param>
        /// <param name="dosDefender">A reference to a DoS defender service implementation.</param>
        /// <param name="keepConnectionOpen">Optional. A value indicating whether to maintain the connection open after processing a message in the connection.</param>
        /// <param name="tcpListener">Optional. An intance to use as the TCP listener, useful for unit testing.</param>
        protected BaseListener(ILogger logger, BaseListenerOptions options, ISocketConnectionFactory socketConnectionFactory, IDoSDefender dosDefender, bool keepConnectionOpen = true, ITcpListener tcpListener = null)
        {
            logger.ThrowIfNull(nameof(logger));
            options.ThrowIfNull(nameof(options));
            socketConnectionFactory.ThrowIfNull(nameof(socketConnectionFactory));

            DataAnnotationsValidator.ValidateObjectRecursive(options);

            this.dosDefender = dosDefender;
            this.keepConnectionOpen = keepConnectionOpen;
            this.socketConnectionFactory = socketConnectionFactory;

            this.tcpListener = tcpListener ?? new FibulaTcpListener(IPAddress.Any, options?.Port ?? 0);

            this.Logger = logger.ForContext(this.GetType());
        }

        /// <summary>
        /// Event fired when a new connection is enstablished.
        /// </summary>
        public event NewConnectionDelegate NewConnection;

        /// <summary>
        /// Gets the logger in use.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Begins listening for requests.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous listening operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    this.tcpListener.Start();

                    // Stop() makes AcceptSocketAsync() throw an ObjectDisposedException.
                    // This means that when the token is cancelled, the callback action here will be to Stop() the listener,
                    // which in turn throws the exception and it gets caught below, exiting gracefully.
                    cancellationToken.Register(() => this.tcpListener.Stop());

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var socket = await this.tcpListener.AcceptSocketAsync().ConfigureAwait(false);

                            ISocketConnection socketConnection = this.socketConnectionFactory.Create(socket);

                            if (this.dosDefender?.IsBlocked(socketConnection.SocketIp) ?? false)
                            {
                                // TODO: evaluate if it is worth just leaving the connection open but ignore it, so that they think they are successfully DoSing...
                                // But we would need to think if it is a connection drain attack then...
                                socketConnection.Close();

                                continue;
                            }

                            this.NewConnection?.Invoke(socketConnection);

                            socketConnection.Closed += this.OnConnectionClose;
                            socketConnection.PacketProcessed += this.AfterConnectionMessageProcessed;

                            this.dosDefender?.LogConnectionAttempt(socketConnection.SocketIp);

                            socketConnection.Read();
                        }
                        catch (ObjectDisposedException)
                        {
                            // This is normal when the listerner is stopped because of token cancellation.
                            break;
                        }
                    }
                }
                catch (SocketException socEx)
                {
                    this.Logger.Error(socEx.ToString());
                }
            });

            // return this to allow other IHostedService-s to start.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the listener.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.tcpListener.Stop();

            // Unhook all subscribers.
            this.NewConnection = null;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs after processing a message from the connection.
        /// </summary>
        /// <param name="connection">The connection where the message is from.</param>
        private void AfterConnectionMessageProcessed(IConnection connection)
        {
            if (!this.keepConnectionOpen)
            {
                connection.Close();
                return;
            }

            connection.Read();
        }

        /// <summary>
        /// Handles a connection's close event.
        /// </summary>
        /// <param name="connection">The connection that was closed.</param>
        private void OnConnectionClose(IConnection connection)
        {
            if (connection == null)
            {
                this.Logger.Warning($"{nameof(this.OnConnectionClose)} called with a null {nameof(connection)} argument.");

                return;
            }

            // De-subscribe to this connection's events.
            connection.Closed -= this.OnConnectionClose;
            connection.PacketProcessed -= this.AfterConnectionMessageProcessed;
        }
    }
}
