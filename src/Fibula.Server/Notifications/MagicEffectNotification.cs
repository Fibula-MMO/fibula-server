// -----------------------------------------------------------------
// <copyright file="MagicEffectNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for magic effects.
    /// </summary>
    public class MagicEffectNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MagicEffectNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="location">The location of the animated text.</param>
        /// <param name="effect">The effect.</param>
        public MagicEffectNotification(
            Func<IEnumerable<IPlayer>> findTargetPlayers,
            Location location,
            AnimatedEffect effect = AnimatedEffect.None)
            : base(findTargetPlayers)
        {
            this.Location = location;
            this.Effect = effect;
        }

        /// <summary>
        /// Gets the location of the animated text.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the actual effect.
        /// </summary>
        public AnimatedEffect Effect { get; }

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
                        MagicEffect = new MagicEffect()
                        {
                            Effect = (uint)this.Effect,
                            Location = new Common.Contracts.Grpc.Location()
                            {
                                X = (ulong)this.Location.X,
                                Y = (ulong)this.Location.Y,
                                Z = (uint)this.Location.Z,
                            },
                        },
                    });
        }
    }
}
