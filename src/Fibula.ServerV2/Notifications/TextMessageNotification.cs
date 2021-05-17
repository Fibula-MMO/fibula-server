// -----------------------------------------------------------------
// <copyright file="TextMessageNotification.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for text messages.
    /// </summary>
    public class TextMessageNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextMessageNotification"/> class.
        /// </summary>
        /// <param name="targetPlayers">The target players of this notification.</param>
        /// <param name="type">The type of text to send.</param>
        /// <param name="message">The text to send.</param>
        public TextMessageNotification(IEnumerable<IPlayer> targetPlayers, MessageType type, string message)
            : base(targetPlayers)
        {
            this.Type = type;
            this.Text = message;
        }

        /// <summary>
        /// Gets the text message type.
        /// </summary>
        public MessageType Type { get; }

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
            return new TextMessagePacket(this.Type, this.Text).YieldSingleItem();
        }
    }
}
