// -----------------------------------------------------------------
// <copyright file="PlayerConditionsPacketWriter.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772.PacketWriters
{
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Definitions.Enumerations;
    using Fibula.Protocol.V772.Extensions;
    using Fibula.Server.Contracts.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a player conditions packet writer for the game server.
    /// </summary>
    public class PlayerConditionsPacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerConditionsPacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public PlayerConditionsPacketWriter(ILogger<PlayerConditionsPacketWriter> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Writes a packet to the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="packet">The packet to write.</param>
        /// <param name="message">The message to write into.</param>
        public override void WriteToMessage(IOutboundPacket packet, ref INetworkMessage message)
        {
            if (!(packet is PlayerConditionsPacket playerConditionsPacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            message.AddByte(playerConditionsPacket.PacketType.ToByte());

            byte conditionFlags = 0;

            // Add condition icons supported by this version.
            if (playerConditionsPacket.Player.HasCondition(ConditionType.Posioned))
            {
                conditionFlags |= 1 << 0;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.Burning))
            {
                conditionFlags |= 1 << 1;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.Electrified))
            {
                conditionFlags |= 1 << 2;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.Drunk))
            {
                conditionFlags |= 1 << 3;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.ManaShield))
            {
                conditionFlags |= 1 << 4;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.Paralyzed))
            {
                conditionFlags |= 1 << 5;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.ChangedSpeed))
            {
                conditionFlags |= 1 << 6;
            }

            if (playerConditionsPacket.Player.HasCondition(ConditionType.InFight))
            {
                conditionFlags |= 1 << 7;
            }

            message.AddByte(conditionFlags);
        }
    }
}
