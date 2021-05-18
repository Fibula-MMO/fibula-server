// -----------------------------------------------------------------
// <copyright file="WorldLightChangedNotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Notifications
{
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Constants;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for a world light change.
    /// </summary>
    public class WorldLightChangedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldLightChangedNotification"/> class.
        /// </summary>
        /// <param name="targetPlayers">The target players of this notification.</param>
        /// <param name="lightLevel">The new world light level.</param>
        /// <param name="lightColor">The new world light color.</param>
        public WorldLightChangedNotification(IEnumerable<IPlayer> targetPlayers, byte lightLevel, byte lightColor = LightConstants.WhiteColor)
            : base(targetPlayers)
        {
            this.LightLevel = lightLevel;
            this.LightColor = lightColor;
        }

        /// <summary>
        /// Gets the new world light level.
        /// </summary>
        public byte LightLevel { get; }

        /// <summary>
        /// Gets the new world light color.
        /// </summary>
        public byte LightColor { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new WorldLightPacket(this.LightLevel, this.LightColor).YieldSingleItem();
        }
    }
}
