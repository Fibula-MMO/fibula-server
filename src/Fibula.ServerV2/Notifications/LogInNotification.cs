// -----------------------------------------------------------------
// <copyright file="LogInNotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Notifications
{
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for when a player first logs in to the game.
    /// </summary>
    public class LogInNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogInNotification"/> class.
        /// </summary>
        /// <param name="player">The player that was logged in.</param>
        /// <param name="atLocation">The location to which the creature is being added.</param>
        /// <param name="descriptionTiles">The map tiles that are being sent as part of the description.</param>
        /// <param name="addEffect">Optional. An effect to add when removing the creature.</param>
        public LogInNotification(
            IPlayer player,
            Location atLocation,
            IEnumerable<ITile> descriptionTiles,
            AnimatedEffect addEffect = AnimatedEffect.None)
            : base(player.YieldSingleItem())
        {
            this.DescriptionTiles = descriptionTiles;
            this.Player = player;
            this.AtLocation = atLocation;
            this.AddEffect = addEffect;
        }

        /// <summary>
        /// Gets the player being logged in.
        /// </summary>
        public IPlayer Player { get; }

        /// <summary>
        /// Gets the location to which the creature is being added.
        /// </summary>
        public Location AtLocation { get; }

        /// <summary>
        /// Gets the tiles that will be sent as part of the description.
        /// </summary>
        public IEnumerable<ITile> DescriptionTiles { get; }

        /// <summary>
        /// Gets the effect to send when adding the creature.
        /// </summary>
        public AnimatedEffect AddEffect { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            var packets = new List<IOutboundPacket>
            {
                new MapDescriptionPacket(this.Player, this.AtLocation, this.DescriptionTiles),
            };

            if (this.AddEffect != AnimatedEffect.None)
            {
                packets.Add(new MagicEffectPacket(this.AtLocation, this.AddEffect));
            }

            return packets;
        }
    }
}
