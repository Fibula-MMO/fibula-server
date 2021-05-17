// -----------------------------------------------------------------
// <copyright file="MapFullDescriptionNotification.cs" company="2Dudes">
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
    using System.Buffers;
    using System.Collections.Generic;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a notification for a full description of the map at a given location.
    /// </summary>
    public class MapFullDescriptionNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapFullDescriptionNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="location">The location at which the description is centered.</param>
        /// <param name="descriptionBytes">The content of the description.</param>
        public MapFullDescriptionNotification(
            Func<IEnumerable<IPlayer>> findTargetPlayers,
            Location location,
            ReadOnlySequence<byte> descriptionBytes)
            : base(findTargetPlayers)
        {
            this.Location = location;
            this.DescriptionBytes = descriptionBytes;
        }

        /// <summary>
        /// Gets the location at which the description starts.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the description bytes.
        /// </summary>
        public ReadOnlySequence<byte> DescriptionBytes { get; }

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

            var description = new Common.Contracts.Grpc.MapDescription()
            {
                Description = Google.Protobuf.ByteString.CopyFrom(this.DescriptionBytes.ToArray()),
            };

            // description.CreaturesBeingAdded.AddRange(this.AddedCreatureIds);
            // description.CreaturesBeingRemoved.AddRange(this.RemovedCreatureIds);
            return targetBuffer.Post(
                    new GameNotification()
                    {
                        MapDescriptionFull = new MapDescriptionFull()
                        {
                            Origin = new Common.Contracts.Grpc.Location()
                            {
                                X = (ulong)this.Location.X,
                                Y = (ulong)this.Location.Y,
                                Z = (uint)this.Location.Z,
                            },
                            Description = description,
                        },
                    });
        }
    }
}
