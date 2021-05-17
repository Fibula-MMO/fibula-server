// -----------------------------------------------------------------
// <copyright file="GameListener.cs" company="2Dudes">
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
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Security.Contracts.Abstractions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class that extends the standard <see cref="BaseTcpListener"/> for a game listener.
    /// </summary>
    /// <typeparam name="TConnectionFactory">The type of connection factory to use in the listener.</typeparam>
    public class GameListener<TConnectionFactory> : BaseTcpListener
        where TConnectionFactory : class, ISocketConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameListener{T}"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="options">The options for this listener.</param>
        /// <param name="connectionFactory">A reference to the connection factory in play here.</param>
        /// <param name="dosDefender">A reference to the DoS defender service implementation.</param>
        public GameListener(
            ILogger<GameListener<TConnectionFactory>> logger,
            IOptions<GameListenerOptions> options,
            TConnectionFactory connectionFactory,
            IDoSDefender dosDefender)
            : base(logger, options?.Value, connectionFactory, dosDefender)
        {
        }
    }
}
