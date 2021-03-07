// -----------------------------------------------------------------
// <copyright file="ProjectilePacketWriter.cs" company="2Dudes">
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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a projectile effect packet writer for the game server.
    /// </summary>
    public class ProjectilePacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectilePacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public ProjectilePacketWriter(ILogger logger)
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
            if (packet is not ProjectilePacket projectilePacket)
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            if (projectilePacket.Effect == ProjectileType.None)
            {
                this.Logger.LogDebug($"Ignoring {packet.GetType().Name} with {ProjectileType.None} effect.");

                return;
            }

            message.AddByte(projectilePacket.PacketType.ToByte());

            message.AddLocation(projectilePacket.FromLocation);
            message.AddLocation(projectilePacket.ToLocation);

            byte valueToSend = projectilePacket.Effect switch
            {
                ProjectileType.Spear => 0x01,
                ProjectileType.Bolt => 0x02,
                ProjectileType.Arrow => 0x03,
                ProjectileType.OrangeOrb => 0x04,
                ProjectileType.BlueOrb => 0x05,
                ProjectileType.PoisonArrow => 0x06,
                ProjectileType.BurstArrow => 0x07,
                ProjectileType.ThrowingStar => 0x08,
                ProjectileType.ThrowingKnife => 0x09,
                ProjectileType.SmallStone => 0x10,
                ProjectileType.BlackOrb => 0x11,
                ProjectileType.LargeRock => 0x12,
                ProjectileType.Snowball => 0x13,
                ProjectileType.PowerBolt => 0x14,
                ProjectileType.GreenOrb => 0x15,
                _ => 0x00,
            };

            message.AddByte(valueToSend);
        }
    }
}
