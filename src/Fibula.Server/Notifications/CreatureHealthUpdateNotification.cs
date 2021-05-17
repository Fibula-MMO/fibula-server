// -----------------------------------------------------------------
// <copyright file="CreatureHealthUpdateNotification.cs" company="2Dudes">
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
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a notification for an update to a creature's health bar.
    /// </summary>
    public class CreatureHealthUpdateNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureHealthUpdateNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="creature">The creature being removed.</param>
        public CreatureHealthUpdateNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, ICreature creature)
            : base(findTargetPlayers)
        {
            creature.ThrowIfNull(nameof(creature));

            this.Creature = creature;
        }

        /// <summary>
        /// Gets a reference to the creature.
        /// </summary>
        public ICreature Creature { get; }

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
                    CreatureHealthUpdate = new CreatureHealthUpdate()
                    {
                        CreatureId = this.Creature.Id,
                        HitpointsStat = new Common.Contracts.Grpc.Stat()
                        {
                            Name = CreatureStat.HitPoints.ToString(),
                            Current = this.Creature.Stats[CreatureStat.HitPoints].Current,
                            Maximum = this.Creature.Stats[CreatureStat.HitPoints].Maximum,
                            Percent = this.Creature.Stats[CreatureStat.HitPoints].Percent,
                        },
                    },
                });
        }
    }
}
