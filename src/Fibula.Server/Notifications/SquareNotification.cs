// -----------------------------------------------------------------
// <copyright file="SquareNotification.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that represents a notification for a world light change.
    /// </summary>
    public class SquareNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquareNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="creatureId">The id of the creature over which to display the square.</param>
        /// <param name="color">The color of the square.</param>
        public SquareNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, uint creatureId, SquareColor color)
            : base(findTargetPlayers)
        {
            this.CreatureId = creatureId;
            this.Color = color;
        }

        /// <summary>
        /// Gets the id of the creature over which to display the square.
        /// </summary>
        public uint CreatureId { get; }

        /// <summary>
        /// Gets the color of the square.
        /// </summary>
        public SquareColor Color { get; }

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
                        Square = new Square()
                        {
                            CreatureId = this.CreatureId,
                            Color = (uint)this.Color,
                        },
                    });
        }
    }
}
