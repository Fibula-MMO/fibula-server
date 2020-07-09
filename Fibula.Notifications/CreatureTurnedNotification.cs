﻿// -----------------------------------------------------------------
// <copyright file="CreatureTurnedNotification.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Notifications
{
    using System;
    using System.Collections.Generic;
    using Fibula.Common.Contracts.Enumerations;
    using Fibula.Common.Utilities;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Creatures.Contracts.Abstractions;
    using Fibula.Notifications.Arguments;
    using Fibula.Notifications.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a notification for when a creature has turned.
    /// </summary>
    public class CreatureTurnedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureTurnedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="arguments">The arguments for this notification.</param>
        public CreatureTurnedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, CreatureTurnedNotificationArguments arguments)
            : base(findTargetPlayers)
        {
            arguments.ThrowIfNull(nameof(arguments));

            this.Arguments = arguments;
        }

        /// <summary>
        /// Gets this notification's arguments.
        /// </summary>
        public CreatureTurnedNotificationArguments Arguments { get; }

        /// <summary>
        /// Finalizes the notification in preparation to it being sent.
        /// </summary>
        /// <param name="context">The context of this notification.</param>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of <see cref="IOutboundPacket"/>s, the ones to be sent.</returns>
        protected override IEnumerable<IOutboundPacket> Prepare(INotificationContext context, IPlayer player)
        {
            var packets = new List<IOutboundPacket>();

            if (this.Arguments.TurnedEffect != AnimatedEffect.None)
            {
                packets.Add(new MagicEffectPacket(this.Arguments.Creature.Location, this.Arguments.TurnedEffect));
            }

            packets.Add(new CreatureTurnedPacket(this.Arguments.Creature, this.Arguments.StackPosition));

            return packets;
        }
    }
}
