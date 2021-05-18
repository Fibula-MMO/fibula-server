// -----------------------------------------------------------------
// <copyright file="AnimatedTextNotification.cs" company="2Dudes">
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
    using System;
    using System.Collections.Generic;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for animated effects.
    /// </summary>
    public class AnimatedTextNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedTextNotification"/> class.
        /// </summary>
        /// <param name="spectators">The spectators of the text.</param>
        /// <param name="location">The location of the animated text.</param>
        /// <param name="color">The color of text to send.</param>
        /// <param name="text">The text to send.</param>
        public AnimatedTextNotification(IEnumerable<IPlayer> spectators, Location location, TextColor color, string text)
            : base(spectators)
        {
            if (color == TextColor.None)
            {
                throw new ArgumentException($"Invalid value for animated text parameter: {color}.", nameof(color));
            }

            this.Location = location;
            this.Color = color;
            this.Text = text;
        }

        /// <summary>
        /// Gets the location of the animated text.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the text color.
        /// </summary>
        public TextColor Color { get; }

        /// <summary>
        /// Gets the text value.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new AnimatedTextPacket(this.Location, this.Color, this.Text).YieldSingleItem();
        }
    }
}
