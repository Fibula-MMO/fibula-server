// -----------------------------------------------------------------
// <copyright file="ContainerClosedNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for a container being closed.
    /// </summary>
    public class ContainerClosedNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerClosedNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="withId">The id with which the container was opened, as perceived by the player.</param>
        public ContainerClosedNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, byte withId)
            : base(findTargetPlayers)
        {
            this.ContainerId = withId;
        }

        /// <summary>
        /// Gets the id with which the container was opened.
        /// </summary>
        public byte ContainerId { get; }

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
                ContainerClose = new ContainerClose()
                {
                    ContainerId = this.ContainerId,
                },
            };

            return targetBuffer.Post(notification);
        }
    }
}
