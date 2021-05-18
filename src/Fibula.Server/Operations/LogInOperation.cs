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

namespace Fibula.Server.Operations
{
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Models;
    using Fibula.Server.Notifications;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a login operation.
    /// </summary>
    public class LogInOperation : Operation
    {
        /// <summary>
        /// The current light level of the world, to send with the login information.
        /// </summary>
        private readonly byte currentWorldLightLevel;

        /// <summary>
        /// The current light color of the world, to send with the login information.
        /// </summary>
        private readonly byte currentWorldLightColor;

        /// <summary>
        /// The player metadata.
        /// </summary>
        private readonly CharacterEntity playerMetadata;

        /// <summary>
        /// The preselected id for the player.
        /// </summary>
        private readonly uint preselectedPlayerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the action.</param>
        /// <param name="reservedId">The id reserved by the game for this player logging in.</param>
        /// <param name="playerMetadata">The creation metadata of the player that is logging in.</param>
        /// <param name="worldLightLevel">The level of the world light to send to the player.</param>
        /// <param name="worldLightColor">The color of the world light to send to the player.</param>
        public LogInOperation(uint requestorId, uint reservedId, CharacterEntity playerMetadata, byte worldLightLevel, byte worldLightColor)
            : base(requestorId)
        {
            reservedId.ThrowIfDefaultValue(nameof(reservedId));

            this.currentWorldLightLevel = worldLightLevel;
            this.currentWorldLightColor = worldLightColor;

            this.playerMetadata = playerMetadata;
            this.preselectedPlayerId = reservedId;
        }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            var targetLoginLocation = this.playerMetadata.LastLocation;

            if (context.Map[targetLoginLocation] == null)
            {
                targetLoginLocation = this.playerMetadata.StartingLocation;
            }

            var creationArguments = new PlayerCreationArguments()
            {
                Type = CreatureType.Player,
                Metadata = this.playerMetadata,
                PreselectedId = this.preselectedPlayerId,
            };

            if (!(context.CreatureFactory.CreateCreature(creationArguments) is IPlayer player))
            {
                context.Logger.LogWarning($"Unable to create player instance for {this.playerMetadata.Name}, aborting log in.");

                return;
            }

            if (!context.GameApi.AddCreatureToGame(targetLoginLocation, player))
            {
                // Unable to place the player in the map.
                var disconnectNotification = new DisconnectNotification(
                        player.YieldSingleItem(),
                        "Your character could not be placed on the map.\nPlease try again, or contact an administrator if the issue persists.");

                context.GameApi.SendNotification(disconnectNotification);

                return;
            }

            var descriptionTiles = context.Map.DescribeAt(player, player.Location);

            // TODO: In addition, we need to send the player's inventory, the first time login message + outfit window here if applicable.
            // And any VIP records here.
            this.SendNotification(context, new PlayerLoginNotification(player, player.Location, descriptionTiles, AnimatedEffect.BubbleBlue));
            this.SendNotification(context, new WorldLightChangedNotification(player.YieldSingleItem(), this.currentWorldLightLevel, this.currentWorldLightColor));
            this.SendNotification(context, new CreatureLightUpdateNotification(player.YieldSingleItem(), player));
            this.SendNotification(context, new TextMessageNotification(player.YieldSingleItem(), MessageType.Status, "This is a test message"));
        }
    }
}
