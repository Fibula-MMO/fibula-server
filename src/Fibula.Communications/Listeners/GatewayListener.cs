﻿// -----------------------------------------------------------------
// <copyright file="GatewayListener.cs" company="2Dudes">
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
    /// Class that extends the standard <see cref="BaseTcpListener"/> for the gateway protocol.
    /// </summary>
    /// <typeparam name="TConnectionFactory">The type of connection factory to use in the listener.</typeparam>
    public class GatewayListener<TConnectionFactory> : BaseTcpListener
        where TConnectionFactory : class, ISocketConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayListener{T}"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="options">The options for this listener.</param>
        /// <param name="connectionFactory">A reference to the connection factory in play here.</param>
        /// <param name="dosDefender">A reference to the DoS defender service implementation.</param>
        public GatewayListener(
            ILogger<GatewayListener<TConnectionFactory>> logger,
            IOptions<GatewayListenerOptions> options,
            TConnectionFactory connectionFactory,
            IDoSDefender dosDefender)
            : base(logger, options?.Value, connectionFactory, dosDefender, keepConnectionOpen: false)
        {
        }
    }
}
