﻿// -----------------------------------------------------------------
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
    using System.Threading.Tasks.Dataflow;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a notification for animated effects.
    /// </summary>
    public class AnimatedTextNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedTextNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="location">The location of the animated text.</param>
        /// <param name="color">The color of text to send.</param>
        /// <param name="text">The text to send.</param>
        public AnimatedTextNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, Location location, TextColor color, string text)
            : base(findTargetPlayers)
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
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>True if the notification was posted successfuly, and false otherwise.</returns>
        public override bool Post(INotificationContext context, IPlayer player)
        {
            if (!(context.Buffer is ITargetBlock<GameNotification> targetBuffer))
            {
                return false;
            }

            var notification = new GameNotification()
            {
                AnimatedText = new AnimatedText()
                {
                    Location = new Common.Contracts.Grpc.Location()
                    {
                        X = (ulong)this.Location.X,
                        Y = (ulong)this.Location.Y,
                        Z = (uint)this.Location.Z,
                    },
                    Color = (uint)this.Color,
                    Text = this.Text,
                },
            };

            return targetBuffer.Post(notification);
        }
    }
}