// -----------------------------------------------------------------
// <copyright file="TileUpdatedNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for a tile update.
    /// </summary>
    public class TileUpdatedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileUpdatedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="location">The location of the updated tile.</param>
        /// <param name="descriptionFunction">The function used to get the description of the updated tile.</param>
        public TileUpdatedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, Location location, Func<IPlayer, Location, (IDictionary<string, object> descriptionMetadata, ReadOnlySequence<byte> descriptionData)> descriptionFunction)
            : base(findTargetPlayers)
        {
            this.Location = location;
            this.TileDescriptionFunction = descriptionFunction;
        }

        /// <summary>
        /// Gets the location of the updated tile.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the function that decribes the tile.
        /// </summary>
        public Func<IPlayer, Location, (IDictionary<string, object> descriptionMetadata, ReadOnlySequence<byte> descriptionData)> TileDescriptionFunction { get; }

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

            var (descriptionMetadata, descriptionBytes) = this.TileDescriptionFunction(player, this.Location);

            return targetBuffer.Post(
                    new GameNotification()
                    {
                        TileUpdate = new TileUpdate()
                        {
                            Location = new Common.Contracts.Grpc.Location()
                            {
                                X = (ulong)this.Location.X,
                                Y = (ulong)this.Location.Y,
                                Z = (uint)this.Location.Z,
                            },
                            DeletingTile = descriptionBytes.Length == 0,
                            Description = ByteString.CopyFrom(descriptionBytes.ToArray()),
                        },
                    });
        }
    }
}
