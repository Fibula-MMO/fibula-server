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

namespace Fibula.Mechanics.Notifications
{
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Creatures.Contracts.Abstractions;
    using Fibula.Definitions.Constants;
    using Fibula.Mechanics.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for a world light change.
    /// </summary>
    public class WorldLightChangedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldLightChangedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="lightLevel">The new world light level.</param>
        /// <param name="lightColor">The new world light color.</param>
        public WorldLightChangedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, byte lightLevel, byte lightColor = LightConstants.WhiteColor)
            : base(findTargetPlayers)
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
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of <see cref="IOutboundPacket"/>s, the ones to be sent.</returns>
        protected override IEnumerable<IOutboundPacket> Prepare(INotificationContext context, IPlayer player)
        {
            return new WorldLightPacket(this.LightLevel, this.LightColor).YieldSingleItem();
        }
    }
}
