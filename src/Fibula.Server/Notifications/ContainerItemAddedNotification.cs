// -----------------------------------------------------------------
// <copyright file="ContainerItemAddedNotification.cs" company="2Dudes">
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
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for when an item is added to a container.
    /// </summary>
    public class ContainerItemAddedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerItemAddedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="containerId">The id of the container in which the item was added.</param>
        /// <param name="item">The item that was added.</param>
        public ContainerItemAddedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, byte containerId, IItem item)
            : base(findTargetPlayers)
        {
            item.ThrowIfNull(nameof(item));

            this.ContainerId = containerId;
            this.Item = item;
        }

        /// <summary>
        /// Gets the index within the container at which the item was added.
        /// </summary>
        public byte ContainerId { get; }

        /// <summary>
        /// Gets the reference to the item.
        /// </summary>
        public IItem Item { get; }

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
                ContainerAddItem = new ContainerAddItem()
                {
                    ContainerId = this.ContainerId,
                    Item = new Common.Contracts.Grpc.Item()
                    {
                        Amount = this.Item.Amount,
                        ItemTypeId = this.Item.TypeId,
                        LiquidColor = (int)this.Item.LiquidType,
                    },
                },
            };

            return targetBuffer.Post(notification);
        }
    }
}
