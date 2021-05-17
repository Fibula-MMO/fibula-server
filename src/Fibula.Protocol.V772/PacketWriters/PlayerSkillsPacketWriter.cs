// -----------------------------------------------------------------
// <copyright file="PlayerSkillsPacketWriter.cs" company="2Dudes">
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
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Outgoing;
    using Fibula.Protocol.V772.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a player skills packet writer for the game server.
    /// </summary>
    public class PlayerSkillsPacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerSkillsPacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public PlayerSkillsPacketWriter(ILogger<PlayerSkillsPacketWriter> logger)
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
            if (!(packet is PlayerSkillsPacket playerSkillsPacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            byte noWeaponSkillLevel = 10;           // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.NoWeapon].CurrentLevel);
            byte noWeaponSkillPercent = 0;          // combatantPlayer.Skills[SkillType.NoWeapon].Percent;

            byte bluntWeaponSkillLevel = 10;        // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Club].CurrentLevel);
            byte bluntWeaponSkillPercent = 0;       // combatantPlayer.Skills[SkillType.Club].Percent;

            byte swordWeaponSkillLevel = 10;        // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Sword].CurrentLevel);
            byte swordWeaponSkillPercent = 0;       // combatantPlayer.Skills[SkillType.Sword].Percent;

            byte axeWeaponSkillLevel = 10;        // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Axe].CurrentLevel);
            byte axeWeaponSkillPercent = 0;       // combatantPlayer.Skills[SkillType.Axe].Percent;

            byte rangedWeaponSkillLevel = 10;        // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Ranged].CurrentLevel);
            byte rangedWeaponSkillPercent = 0;       // combatantPlayer.Skills[SkillType.Ranged].Percent;

            byte shieldingSkillLevel = 10;        // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Shield].CurrentLevel);
            byte shieldingSkillPercent = 0;       // combatantPlayer.Skills[SkillType.Shield].Percent;

            byte fishingSkillLevel = 10;        // (byte)Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Fishing].CurrentLevel);
            byte fishingSkillPercent = 0;       // combatantPlayer.Skills[SkillType.Fishing].Percent;

            message.AddByte(playerSkillsPacket.PacketType.ToByte());

            // NoWeapon
            message.AddByte(noWeaponSkillLevel);
            message.AddByte(noWeaponSkillPercent);

            // Club
            message.AddByte(bluntWeaponSkillLevel);
            message.AddByte(bluntWeaponSkillPercent);

            // Sword
            message.AddByte(swordWeaponSkillLevel);
            message.AddByte(swordWeaponSkillPercent);

            // Axe
            message.AddByte(axeWeaponSkillLevel);
            message.AddByte(axeWeaponSkillPercent);

            // Ranged
            message.AddByte(rangedWeaponSkillLevel);
            message.AddByte(rangedWeaponSkillPercent);

            // Shield
            message.AddByte(shieldingSkillLevel);
            message.AddByte(shieldingSkillPercent);

            // Fishing
            message.AddByte(fishingSkillLevel);
            message.AddByte(fishingSkillPercent);
        }
    }
}
