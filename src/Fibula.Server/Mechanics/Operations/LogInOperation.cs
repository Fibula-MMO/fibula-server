// -----------------------------------------------------------------
// <copyright file="LogInOperation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics.Operations
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Creatures;
    using Fibula.Server.Mechanics.Notifications;
    using Fibula.Utilities.Common.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a login operation.
    /// </summary>
    public class LogInOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogInOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the action.</param>
        /// <param name="client">The client requesting the log in.</param>
        /// <param name="playerMetadata">The creation metadata of the player that is logging in.</param>
        /// <param name="worldLightLevel">The level of the world light to send to the player.</param>
        /// <param name="worldLightColor">The color of the world light to send to the player.</param>
        public LogInOperation(uint requestorId, IClient client, CharacterEntity playerMetadata, byte worldLightLevel, byte worldLightColor)
            : base(requestorId)
        {
            this.Client = client;
            this.CurrentWorldLightLevel = worldLightLevel;
            this.CurrentWorldLightColor = worldLightColor;

            this.PlayerMetadata = playerMetadata;
        }

        /// <summary>
        /// Gets the client requesting the log in.
        /// </summary>
        public IClient Client { get; }

        /// <summary>
        /// Gets the current light level of the world, to send with the login information.
        /// </summary>
        public byte CurrentWorldLightLevel { get; }

        /// <summary>
        /// Gets the current light color of the world, to send with the login information.
        /// </summary>
        public byte CurrentWorldLightColor { get; }

        /// <summary>
        /// Gets the player metadata.
        /// </summary>
        public CharacterEntity PlayerMetadata { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            var targetLoginLocation = this.PlayerMetadata.LastLocation;

            if (context.Map.GetTileAt(targetLoginLocation) == null)
            {
                targetLoginLocation = this.PlayerMetadata.StartingLocation;
            }

            var creationArguments = new PlayerCreationArguments()
            {
                Client = this.Client,
                Type = CreatureType.Player,
                Metadata = this.PlayerMetadata,
            };

            if (!(context.CreatureFactory.CreateCreature(creationArguments) is IPlayer player))
            {
                context.Logger.LogWarning($"Unable to create player instance for {this.PlayerMetadata.Name}, aborting log in.");

                return;
            }

            if (!context.GameApi.AddCreatureToGame(targetLoginLocation, player))
            {
                // Unable to place the player in the map.
                var disconnectNotification = new GenericNotification(
                        () => player.YieldSingleItem(),
                        new GameServerDisconnectPacket("Your character could not be placed on the map.\nPlease try again, or contact an administrator if the issue persists."));

                context.Scheduler.ScheduleEvent(disconnectNotification);

                return;
            }

            var (descriptionMetadata, descriptionBytes) = context.MapDescriptor.DescribeAt(player, player.Location);

            // TODO: In addition, we need to send the player's inventory, the first time login message + outfit window here if applicable.
            // And any VIP records here.
            var notification = new GenericNotification(
                () => player.YieldSingleItem(),
                new PlayerLoginPacket(player.Id, player),
                new MapDescriptionPacket(player.Location, descriptionBytes),
                new MagicEffectPacket(player.Location, AnimatedEffect.BubbleBlue),
                new PlayerStatsPacket(player),
                new PlayerSkillsPacket(player),
                new WorldLightPacket(this.CurrentWorldLightLevel, this.CurrentWorldLightColor),
                new CreatureLightPacket(player),
                new TextMessagePacket(MessageType.StatusDefault, "This is a test message"),
                new PlayerConditionsPacket(player));

            if (descriptionMetadata.TryGetValue(IMapDescriptor.CreatureIdsToLearnMetadataKeyName, out object creatureIdsToLearnBoxed) &&
                descriptionMetadata.TryGetValue(IMapDescriptor.CreatureIdsToForgetMetadataKeyName, out object creatureIdsToForgetBoxed) &&
                creatureIdsToLearnBoxed is IEnumerable<uint> creatureIdsToLearn && creatureIdsToForgetBoxed is IEnumerable<uint> creatureIdsToForget)
            {
                notification.Sent += (client) =>
                {
                    foreach (var creatureId in creatureIdsToLearn)
                    {
                        client.AddKnownCreature(creatureId);
                    }

                    foreach (var creatureId in creatureIdsToForget)
                    {
                        client.RemoveKnownCreature(creatureId);
                    }
                };
            }

            this.SendNotification(context, notification);
        }
    }
}
