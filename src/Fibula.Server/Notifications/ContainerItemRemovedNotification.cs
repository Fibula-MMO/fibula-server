// -----------------------------------------------------------------
// <copyright file="ContainerItemRemovedNotification.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a notification for when an item is removed from a container.
    /// </summary>
    public class ContainerItemRemovedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerItemRemovedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="containerId">The id of the container in which the item was updated.</param>
        /// <param name="index">The index within the container at which the removed item was.</param>
        public ContainerItemRemovedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, byte containerId, byte index)
            : base(findTargetPlayers)
        {
            this.ContainerId = containerId;
            this.Index = index;
        }

        /// <summary>
        /// Gets the id of the container from which the item was removed.
        /// </summary>
        public byte ContainerId { get; }

        /// <summary>
        /// Gets the index at which the item was removed.
        /// </summary>
        public byte Index { get; }

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
                ContainerRemoveItem = new ContainerRemoveItem()
                {
                    ContainerId = this.ContainerId,
                    Index = this.Index,
                },
            };

            return targetBuffer.Post(notification);
        }
    }
}
