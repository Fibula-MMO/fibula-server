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
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Creatures;
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

            this.CurrentWorldLightLevel = worldLightLevel;
            this.CurrentWorldLightColor = worldLightColor;

            this.PlayerMetadata = playerMetadata;
            this.PreselectedId = reservedId;
        }

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
        /// Gets the preselected id for the player.
        /// </summary>
        public uint PreselectedId { get; }

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
                Type = CreatureType.Player,
                Metadata = this.PlayerMetadata,
                PreselectedId = this.PreselectedId,
            };

            if (!(context.CreatureFactory.CreateCreature(creationArguments) is IPlayer player))
            {
                context.Logger.LogWarning($"Unable to create player instance for {this.PlayerMetadata.Name}, aborting log in.");

                return;
            }

            if (!context.GameApi.AddCreatureToGame(targetLoginLocation, player))
            {
                // Unable to place the player in the map.
                var disconnectNotification = new DisconnectNotification(
                        () => player.YieldSingleItem(),
                        "Your character could not be placed on the map.\nPlease try again, or contact an administrator if the issue persists.");

                context.GameApi.SendNotification(disconnectNotification);

                return;
            }

            var (descriptionMetadata, descriptionBytes) = context.MapDescriptor.DescribeAt(player, player.Location);

            // TODO: In addition, we need to send the player's inventory, the first time login message + outfit window here if applicable.
            // And any VIP records here.
            this.SendNotification(context, new PlayerLoginNotification(() => player.YieldSingleItem(), player));
            this.SendNotification(context, new MapFullDescriptionNotification(() => player.YieldSingleItem(), player.Location, descriptionBytes));
            this.SendNotification(context, new MagicEffectNotification(() => player.YieldSingleItem(), player.Location, AnimatedEffect.BubbleBlue));
            this.SendNotification(context, new PlayerStatsUpdateNotification(() => player.YieldSingleItem(), player));
            this.SendNotification(context, new PlayerSkillsUpdateNotification(() => player.YieldSingleItem(), player));
            this.SendNotification(context, new WorldLightChangedNotification(() => player.YieldSingleItem(), this.CurrentWorldLightLevel, this.CurrentWorldLightColor));
            this.SendNotification(context, new CreatureLightUpdateNotification(() => player.YieldSingleItem(), player));
            this.SendNotification(context, new TextMessageNotification(() => player.YieldSingleItem(), MessageType.Status, "This is a test message"));
            this.SendNotification(context, new PlayerConditionsUpdateNotification(() => player.YieldSingleItem(), player));
        }
    }
}
