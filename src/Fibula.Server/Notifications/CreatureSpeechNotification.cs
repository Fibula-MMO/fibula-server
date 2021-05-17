// -----------------------------------------------------------------
// <copyright file="CreatureSpeechNotification.cs" company="2Dudes">
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
    /// Class that represents a notification for a creature speech.
    /// </summary>
    public class CreatureSpeechNotification : Notification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureSpeechNotification"/> class.
        /// </summary>
        /// <param name="findTargetPlayers">A function to determine the target players of this notification.</param>
        /// <param name="creature">The creature that spoke.</param>
        /// <param name="speechType">The type of speech.</param>
        /// <param name="channelType">The channel type.</param>
        /// <param name="text">The message content.</param>
        /// <param name="receiver">Optional. The receiver of the message.</param>
        public CreatureSpeechNotification(Func<IEnumerable<IPlayer>> findTargetPlayers, ICreature creature, SpeechType speechType, ChatChannelType channelType, string text, string receiver = "")
            : base(findTargetPlayers)
        {
            this.Creature = creature;
            this.SpeechType = speechType;
            this.ChannelType = channelType;
            this.Text = text;
            this.Receiver = receiver;
        }

        /// <summary>
        /// Gets the id of the creature that spoke.
        /// </summary>
        public ICreature Creature { get; }

        /// <summary>
        /// Gets the type of speech.
        /// </summary>
        public SpeechType SpeechType { get; }

        /// <summary>
        /// Gets the type of channel.
        /// </summary>
        public ChatChannelType ChannelType { get; }

        /// <summary>
        /// Gets the text of the speech.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the receiver of the speech, if this is a private channel message.
        /// </summary>
        public string Receiver { get; }

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
                    CreatureSpeech = new CreatureSpeech()
                    {
                        AtLocation = new Common.Contracts.Grpc.Location()
                        {
                            X = (ulong)this.Creature.Location.X,
                            Y = (ulong)this.Creature.Location.Y,
                            Z = (uint)this.Creature.Location.Z,
                        },
                        SenderId = this.Creature.Id,
                        SenderName = this.Creature.Name,
                        SpeechType = (uint)this.SpeechType,
                        Message = this.Text,
                        Channel = (uint)this.ChannelType,
                        UnixTimestamp = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    },
                });
        }
    }
}
