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

namespace Fibula.Protocol.V772.Extensions
{
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Listeners;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Protocol.Contracts.Abstractions;
    using Fibula.Protocol.V772.PacketReaders;
    using Fibula.Protocol.V772.PacketWriters;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Static class that adds convenient methods to add the concrete implementations contained in this library.
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Adds all the game server components related to protocol 7.72 contained in this library to the services collection.
        /// It also configures any <see cref="IOptions{T}"/> required by any such components.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration loaded.</param>
        public static void AddProtocol772GameServerComponents(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.ThrowIfNull(nameof(configuration));

            // Configure the options required by the services we're about to add.
            services.Configure<GameListenerOptions>(configuration.GetSection(nameof(GameListenerOptions)));

            var packetReadersToAdd = new Dictionary<InboundPacketType, Type>()
            {
                { InboundPacketType.Attack, typeof(AttackPacketReader) },
                { InboundPacketType.AutoMove, typeof(AutoMovePacketReader) },
                { InboundPacketType.AutoMoveCancel, typeof(AutoMoveCancelPacketReader) },
                { InboundPacketType.ChangeModes, typeof(ChangeModesPacketReader) },
                { InboundPacketType.Follow, typeof(FollowPacketReader) },
                { InboundPacketType.Heartbeat, typeof(HeartbeatPacketReader) },
                { InboundPacketType.HeartbeatResponse, typeof(HeartbeatResponsePacketReader) },
                { InboundPacketType.LogIn, typeof(GameLogInPacketReader) },
                { InboundPacketType.LogOut, typeof(GameLogOutPacketReader) },
                { InboundPacketType.LookAt, typeof(LookAtPacketReader) },
                { InboundPacketType.Speech, typeof(SpeechPacketReader) },
                { InboundPacketType.StopAllActions, typeof(StopAllActionsPacketReader) },
                { InboundPacketType.TurnNorth, typeof(TurnNorthPacketReader) },
                { InboundPacketType.TurnEast, typeof(TurnEastPacketReader) },
                { InboundPacketType.TurnSouth, typeof(TurnSouthPacketReader) },
                { InboundPacketType.TurnWest, typeof(TurnWestPacketReader) },
                { InboundPacketType.WalkNorth, typeof(WalkNorthPacketReader) },
                { InboundPacketType.WalkNortheast, typeof(WalkNortheastPacketReader) },
                { InboundPacketType.WalkEast, typeof(WalkEastPacketReader) },
                { InboundPacketType.WalkSoutheast, typeof(WalkSoutheastPacketReader) },
                { InboundPacketType.WalkSouth, typeof(WalkSouthPacketReader) },
                { InboundPacketType.WalkSouthwest, typeof(WalkSouthwestPacketReader) },
                { InboundPacketType.WalkWest, typeof(WalkWestPacketReader) },
                { InboundPacketType.WalkNorthwest, typeof(WalkNorthwestPacketReader) },
            };

            foreach (var (packetType, type) in packetReadersToAdd)
            {
                services.TryAddSingleton(type);
            }

            var packetWritersToAdd = new Dictionary<OutboundPacketType, Type>()
            {
                { OutboundPacketType.AnimatedText, typeof(AnimatedTextPacketWriter) },
                { OutboundPacketType.AddThing, typeof(AddCreaturePacketWriter) },
                { OutboundPacketType.ContainerClose, typeof(ContainerClosePacketWriter) },
                { OutboundPacketType.ContainerOpen, typeof(ContainerOpenPacketWriter) },
                { OutboundPacketType.ContainerAddItem, typeof(ContainerAddItemPacketWriter) },
                { OutboundPacketType.ContainerUpdateItem, typeof(ContainerUpdateItemPacketWriter) },
                { OutboundPacketType.CreatureHealth, typeof(CreatureHealthUpdatePacketWriter) },
                { OutboundPacketType.CreatureLight, typeof(CreatureLightPacketWriter) },
                { OutboundPacketType.CreatureMoved, typeof(CreatureMovedPacketWriter) },
                { OutboundPacketType.CreatureSpeedChange, typeof(CreatureSpeedChangePacketWriter) },
                { OutboundPacketType.UpdateThing, typeof(CreatureTurnedPacketWriter) },
                { OutboundPacketType.CreatureSpeech, typeof(CreatureSpeechPacketWriter) },
                { OutboundPacketType.GameDisconnect, typeof(GameServerDisconnectPacketWriter) },
                { OutboundPacketType.Heartbeat, typeof(HeartbeatPacketWriter) },
                { OutboundPacketType.HeartbeatResponse, typeof(HeartbeatResponsePacketWriter) },
                { OutboundPacketType.MagicEffect, typeof(MagicEffectPacketWriter) },
                { OutboundPacketType.MapDescription, typeof(MapDescriptionPacketWriter) },
                { OutboundPacketType.MapSliceNorth, typeof(MapPartialDescriptionPacketWriter) },
                { OutboundPacketType.MapSliceEast, typeof(MapPartialDescriptionPacketWriter) },
                { OutboundPacketType.MapSliceSouth, typeof(MapPartialDescriptionPacketWriter) },
                { OutboundPacketType.MapSliceWest, typeof(MapPartialDescriptionPacketWriter) },
                { OutboundPacketType.PlayerConditions, typeof(PlayerConditionsPacketWriter) },
                { OutboundPacketType.InventoryEmpty, typeof(PlayerInventoryClearSlotPacketWriter) },
                { OutboundPacketType.InventoryItem, typeof(PlayerInventorySetSlotPacketWriter) },
                { OutboundPacketType.PlayerSkills, typeof(PlayerSkillsPacketWriter) },
                { OutboundPacketType.PlayerStats, typeof(PlayerStatsPacketWriter) },
                { OutboundPacketType.ProjectileEffect, typeof(ProjectilePacketWriter) },
                { OutboundPacketType.RemoveThing, typeof(RemoveAtLocationPacketWriter) },
                { OutboundPacketType.Square, typeof(SquarePacketWriter) },
                { OutboundPacketType.PlayerLogin, typeof(PlayerLoginPacketWriter) },
                { OutboundPacketType.TextMessage, typeof(TextMessagePacketWriter) },
                { OutboundPacketType.TileUpdate, typeof(TileUpdatePacketWriter) },
                { OutboundPacketType.CancelAttack, typeof(PlayerCancelAttackPacketWriter) },
                { OutboundPacketType.CancelWalk, typeof(PlayerCancelWalkPacketWriter) },
                { OutboundPacketType.WorldLight, typeof(WorldLightPacketWriter) },
            };

            foreach (var (packetType, type) in packetWritersToAdd)
            {
                services.TryAddSingleton(type);
            }

            services.AddSingleton(s =>
            {
                var protocol = new GameProtocol_v772(s.GetRequiredService<ILogger<GameProtocol_v772>>());

                foreach (var (packetType, type) in packetReadersToAdd)
                {
                    protocol.RegisterPacketReader(packetType, s.GetRequiredService(type) as IPacketReader);
                }

                foreach (var (packetType, type) in packetWritersToAdd)
                {
                    protocol.RegisterPacketWriter(packetType, s.GetRequiredService(type) as IPacketWriter);
                }

                return protocol;
            });

            services.TryAddSingleton<ITileDescriptor, TileDescriptor_v772>();

            services.TryAddSingleton<IPredefinedItemSet, PredefinedItemSet_v772>();

            services.TryAddSingleton<Client772ConnectionFactory<GameProtocol_v772>>();
            services.TryAddSingleton<ISocketConnectionFactory>(s => s.GetService<Client772ConnectionFactory<GameProtocol_v772>>());

            services.TryAddSingleton<GameListener<Client772ConnectionFactory<GameProtocol_v772>>>();
            services.AddSingleton<IListener>(s => s.GetService<GameListener<Client772ConnectionFactory<GameProtocol_v772>>>());

            // Since they are derived from IHostedService should be also registered as such.
            services.AddHostedService(s => s.GetService<GameListener<Client772ConnectionFactory<GameProtocol_v772>>>());
        }

        /// <summary>
        /// Adds all the gateway server components related to protocol 7.72 contained in this library to the services collection.
        /// It also configures any <see cref="IOptions{T}"/> required by any such components.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration loaded.</param>
        public static void AddProtocol772GatewayServerComponents(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.ThrowIfNull(nameof(configuration));

            // Configure the options required by the services we're about to add.
            services.Configure<GatewayListenerOptions>(configuration.GetSection(nameof(GatewayListenerOptions)));

            // Add all handlers
            services.TryAddSingleton<GatewayLogInPacketReader>();

            var packetWritersToAdd = new Dictionary<OutboundPacketType, Type>()
            {
                { OutboundPacketType.CharacterList, typeof(CharacterListPacketWriter) },
                { OutboundPacketType.GatewayDisconnect, typeof(GatewayServerDisconnectPacketWriter) },
                { OutboundPacketType.MessageOfTheDay, typeof(MessageOfTheDayPacketWriter) },
            };

            foreach (var (packetType, type) in packetWritersToAdd)
            {
                services.TryAddSingleton(type);
            }

            services.AddSingleton(s =>
            {
                var protocol = new GatewayProtocol_v772(s.GetRequiredService<ILogger<GatewayProtocol_v772>>());

                protocol.RegisterPacketReader(InboundPacketType.LogIn, s.GetRequiredService<GatewayLogInPacketReader>());

                foreach (var (packetType, type) in packetWritersToAdd)
                {
                    protocol.RegisterPacketWriter(packetType, s.GetRequiredService(type) as IPacketWriter);
                }

                return protocol;
            });

            services.TryAddSingleton<Client772ConnectionFactory<GatewayProtocol_v772>>();
            services.TryAddSingleton<ISocketConnectionFactory>(s => s.GetService<Client772ConnectionFactory<GatewayProtocol_v772>>());

            services.TryAddSingleton<GatewayListener<Client772ConnectionFactory<GatewayProtocol_v772>>>();
            services.AddSingleton<IListener>(s => s.GetService<GatewayListener<Client772ConnectionFactory<GatewayProtocol_v772>>>());

            // Since they are derived from IHostedService should be also registered as such.
            services.AddHostedService(s => s.GetService<GatewayListener<Client772ConnectionFactory<GatewayProtocol_v772>>>());
        }
    }
}
