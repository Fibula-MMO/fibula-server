// -----------------------------------------------------------------
// <copyright file="ProjectileNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for projectile effects.
    /// </summary>
    public class ProjectileNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectileNotification"/> class.
        /// </summary>
        /// <param name="spectators">The spectators that saw the projectile.</param>
        /// <param name="fromLocation">The location from which the projectile originates.</param>
        /// <param name="toLocation">The location to which the projectile travels.</param>
        /// <param name="type">The type of projectile.</param>
        public ProjectileNotification(
            IEnumerable<IPlayer> spectators,
            Location fromLocation,
            Location toLocation,
            ProjectileType type = ProjectileType.None)
            : base(spectators)
        {
            this.FromLocation = fromLocation;
            this.ToLocation = toLocation;
            this.Type = type;
        }

        /// <summary>
        /// Gets the actual projectile type.
        /// </summary>
        public ProjectileType Type { get; }

        /// <summary>
        /// Gets the location from which the projectile originates.
        /// </summary>
        public Location FromLocation { get; }

        /// <summary>
        /// Gets the location to which the projectile travels.
        /// </summary>
        public Location ToLocation { get; }

        /// <summary>
        /// Prepares the packets that will be sent out because of this notification, for the given player.
        /// </summary>
        /// <param name="player">The player which this notification is being prepared for.</param>
        /// <returns>A collection of packets to be sent out to the player.</returns>
        public override IEnumerable<IOutboundPacket> PrepareFor(IPlayer player)
        {
            return new ProjectilePacket(this.FromLocation, this.ToLocation, this.Type).YieldSingleItem();
        }
    }
}
