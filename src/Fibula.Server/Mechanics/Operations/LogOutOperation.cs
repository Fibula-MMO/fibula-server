// -----------------------------------------------------------------
// <copyright file="LogOutOperation.cs" company="2Dudes">
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
    using System;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Extensions;
    using Fibula.Server.Notifications;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a logout operation.
    /// </summary>
    public class LogOutOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogOutOperation"/> class.
        /// </summary>
        /// <param name="requestorId">The id of the creature requesting the use.</param>
        /// <param name="player">The player being logged out.</param>
        public LogOutOperation(uint requestorId, IPlayer player)
            : base(requestorId)
        {
            this.Player = player;
        }

        /// <summary>
        /// Gets the player to log out.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Executes the operation's logic.
        /// </summary>
        /// <param name="context">A reference to the operation context.</param>
        protected override void Execute(IOperationContext context)
        {
            if (!this.Player.IsDead && this.Player.HasCondition(ConditionType.InFight))
            {
                this.SendNotification(
                    context,
                    new TextMessageNotification(
                        () => this.Player.YieldSingleItem(),
                        MessageType.StatusSmall,
                        "You may not logout during or immediately after a fight."));

                return;
            }

            if (!context.Map.GetTileAt(this.Player.Location, out ITile tile))
            {
                return;
            }

            // TODO: more validations missing
            using var uow = context.ApplicationContext.CreateNewUnitOfWork();

            if (!(uow.Characters.FindByName(this.Player.Name) is CharacterEntity characterEntity))
            {
                throw new InvalidOperationException($"Unable to find entity for player {this.Player.Name}.");
            }

            // At this point, we're allowed to log this player out, so go for it.
            var playerLocation = this.Player.Location;
            var saveAtLocation = this.Player.IsDead ? characterEntity.StartingLocation : playerLocation;
            var removedFromMap = context.GameApi.RemoveCreatureFromGame(this.Player);

            if (removedFromMap || this.Player.IsDead)
            {
                if (!this.Player.IsDead)
                {
                    this.SendNotification(context, new MagicEffectNotification(() => context.Map.FindPlayersThatCanSee(playerLocation), playerLocation, AnimatedEffect.Puff));
                }

                this.SendNotification(context, new PlayerLogoutNotification(() => this.Player.YieldSingleItem(), this.Player));

                context.CreatureManager.UnregisterCreature(this.Player);

                characterEntity.LastLocation = saveAtLocation;

                uow.Complete();
            }
        }
    }
}
