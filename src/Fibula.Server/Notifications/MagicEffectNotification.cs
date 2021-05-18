// -----------------------------------------------------------------
// <copyright file="MagicEffectNotification.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;

    /// <summary>
    /// Class that represents a notification for magic effects.
    /// </summary>
    public class MagicEffectNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MagicEffectNotification"/> class.
        /// </summary>
        /// <param name="spectators">The set of players that spectated the magic effect.</param>
        /// <param name="location">The location of the animated text.</param>
        /// <param name="effect">The effect.</param>
        public MagicEffectNotification(
            IEnumerable<IPlayer> spectators,
            Location location,
            AnimatedEffect effect = AnimatedEffect.None)
            : base(spectators)
        {
            this.Location = location;
            this.Effect = effect;
        }

        /// <summary>
        /// Gets the location of the animated text.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the actual effect.
        /// </summary>
        public AnimatedEffect Effect { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new MagicEffectPacket(this.Location, this.Effect).YieldSingleItem();
        }
    }
}
