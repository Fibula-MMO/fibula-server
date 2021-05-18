// -----------------------------------------------------------------
// <copyright file="CompositionRootExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Extensions
{
    using System;
    using System.Collections.Generic;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Protocol.V772.Extensions;
    using Fibula.Security.Extensions;
    using Fibula.TcpServer.Contracts.Abstractions;
    using Fibula.TcpServer.Handlers;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Static class that adds convenient methods to add the concrete implementations contained in this library.
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Adds all concrete class implementations contained in this library to the services collection.
        /// Additionally, registers the options related to the concrete implementations added, such as:
        ///     <see cref="TcpServerOptions"/>.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration reference.</param>
        public static void AddTcpServer(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.ThrowIfNull(nameof(configuration));

            // Configure options here
            services.Configure<TcpServerOptions>(configuration.GetSection(nameof(TcpServerOptions)));

            services.AddSimpleDosDefender(configuration);
            services.AddLocalPemFileRsaDecryptor(configuration);

            services.AddHandlers();

            // TODO: This is fixed to a version here. We can expand on selecting a version by
            // passing down an argument and choosing to inject a server version here based on it.
            services.AddProtocol772GameServerComponents(configuration);

            // TODO: We do not necessarily want the gateway TCP server running in the same box as the
            // game TCP server. If we split this, we also need to modify AddHandlers() below.
            services.AddProtocol772GatewayServerComponents(configuration);

            // services.AddSingleton<IMapDescriptor, MapDescriptor>();
            services.AddSingleton<IClientsManager, ClientsManager>();

            services.AddSingleton<TcpServer>();
            services.AddSingleton<ITcpServer>(s => s.GetService<TcpServer>());

            // Those derived from IHostedService must be added using AddHostedService.
            services.AddHostedService(s => s.GetService<TcpServer>());
        }

        private static void AddHandlers(this IServiceCollection services)
        {
            var packetTypeToHandlersMap = new Dictionary<Type, Type>()
            {
                { typeof(IAttackInfo), typeof(AttackHandler) },
                { typeof(IAutoMovementInfo), typeof(AutoMoveHandler) },
                { typeof(IActionWithoutContentInfo), typeof(ActionWithoutContentHandler) },
                { typeof(IBytesInfo), typeof(DefaultRequestHandler) },
                { typeof(IFollowInfo), typeof(FollowHandler) },
                { typeof(IGameLogInInfo), typeof(GameLogInHandler) },
                { typeof(IGatewayLoginInfo), typeof(GatewayLogInHandler) },
                { typeof(ILookAtInfo), typeof(LookAtHandler) },
                { typeof(IModesInfo), typeof(ModesHandler) },
                { typeof(ISpeechInfo), typeof(SpeechHandler) },
                { typeof(ITurnOnDemandInfo), typeof(TurnOnDemandHandler) },
                { typeof(IWalkOnDemandInfo), typeof(WalkOnDemandHandler) },
            };

            foreach (var (packetType, type) in packetTypeToHandlersMap)
            {
                services.TryAddSingleton(type);
            }

            services.AddSingleton<IHandlerSelector>(s =>
            {
                var handlerSelector = new HandlerSelector(s.GetRequiredService<Microsoft.Extensions.Logging.ILogger<HandlerSelector>>());

                foreach (var (packetType, type) in packetTypeToHandlersMap)
                {
                    handlerSelector.RegisterForPacketType(packetType, s.GetRequiredService(type) as ITcpRequestHandler);
                }

                return handlerSelector;
            });
        }
    }
}
