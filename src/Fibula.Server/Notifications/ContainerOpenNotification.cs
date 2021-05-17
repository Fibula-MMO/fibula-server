// -----------------------------------------------------------------
// <copyright file="ContainerOpenNotification.cs" company="2Dudes">
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
    using System.Linq;
    using System.Threading.Tasks.Dataflow;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for a container being opened.
    /// </summary>
    public class ContainerOpenNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerOpenNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="withId">The id with which the container was opened, as perceived by the player.</param>
        /// <param name="container">The container.</param>
        public ContainerOpenNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, byte withId, IContainerItem container)
            : base(findTargetPlayers)
        {
            container.ThrowIfNull(nameof(container));

            this.ContainerId = withId;
            this.Container = container;
        }

        /// <summary>
        /// Gets the id with which the container was opened.
        /// </summary>
        public byte ContainerId { get; }

        /// <summary>
        /// Gets the reference to the container.
        /// </summary>
        public IContainerItem Container { get; }

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

            var containerOpenPacket = new ContainerOpen()
            {
                ContainerId = this.ContainerId,
                ItemTypeId = this.Container.TypeId,
                Name = this.Container.Type.Name,
                Volume = this.Container.Capacity,
                HasParent = this.Container.ParentContainer is IContainerItem parentContainer && parentContainer.Type.TypeId != 0,
            };

            containerOpenPacket.Items.AddRange(
                this.Container.Content.Select(
                    i =>
                    new Common.Contracts.Grpc.Item()
                    {
                        Amount = i.Amount,
                        ItemTypeId = i.TypeId,
                        LiquidColor = (int)i.LiquidType,
                    }));

            var notification = new GameNotification()
            {
                ContainerOpen = containerOpenPacket,
            };

            return targetBuffer.Post(notification);
        }
    }
}
