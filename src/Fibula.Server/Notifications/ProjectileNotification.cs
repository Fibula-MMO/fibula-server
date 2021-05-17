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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a notification for projectile effects.
    /// </summary>
    public class ProjectileNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectileNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="fromLocation">The location from which the projectile originates.</param>
        /// <param name="toLocation">The location to which the projectile travels.</param>
        /// <param name="type">The type of projectile.</param>
        public ProjectileNotification(
            Func<IEnumerable<IPlayer>> findTargetPlayers,
            Location fromLocation,
            Location toLocation,
            ProjectileType type = ProjectileType.None)
            : base(findTargetPlayers)
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

            return targetBuffer.Post(
                    new GameNotification()
                    {
                        Projectile = new Projectile()
                        {
                            Type = (uint)this.Type,
                            FromLocation = new Common.Contracts.Grpc.Location()
                            {
                                X = (ulong)this.FromLocation.X,
                                Y = (ulong)this.FromLocation.Y,
                                Z = (uint)this.FromLocation.Z,
                            },
                            ToLocation = new Common.Contracts.Grpc.Location()
                            {
                                X = (ulong)this.ToLocation.X,
                                Y = (ulong)this.ToLocation.Y,
                                Z = (uint)this.ToLocation.Z,
                            },
                        },
                    });
        }
    }
}
