// -----------------------------------------------------------------
// <copyright file="CreatureSpeedChangeNotification.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Common.Extensions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for a creature's speed changing.
    /// </summary>
    public class CreatureSpeedChangeNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureSpeedChangeNotification"/> class.
        /// </summary>
        /// <param name="spectators">The players that spectated the speed change.</param>
        /// <param name="creature">The creature which's speed changed.</param>
        public CreatureSpeedChangeNotification(IEnumerable<IPlayer> spectators, ICreature creature)
            : base(spectators)
        {
            creature.ThrowIfNull(nameof(creature));

            this.Creature = creature;
        }

        /// <summary>
        /// Gets a reference to the creature.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new CreatureSpeedChangePacket(this.Creature).YieldSingleItem();
        }
    }
}
