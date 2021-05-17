// -----------------------------------------------------------------
// <copyright file="PlayerStatsPacketWriter.cs" company="2Dudes">
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
    /// Class that represents a player stats packet writer for the game server.
    /// </summary>
    public class PlayerStatsPacketWriter : BasePacketWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStatsPacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public PlayerStatsPacketWriter(ILogger<PlayerStatsPacketWriter> logger)
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
            if (!(packet is PlayerStatsPacket playerStatsPacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            ushort hitpoints = 100; // Math.Min(ushort.MaxValue, (ushort)playerStatsPacket.Player.Stats[CreatureStat.HitPoints].Current);
            ushort maxHitpoints = 100; // Math.Min(ushort.MaxValue, (ushort)playerStatsPacket.Player.Stats[CreatureStat.HitPoints].Maximum);
            ushort manapoints = 50; //  Math.Min(ushort.MaxValue, (ushort)playerStatsPacket.Player.Stats[CreatureStat.ManaPoints].Current);
            ushort maxManapoints = 50; // Math.Min(ushort.MaxValue, (ushort)playerStatsPacket.Player.Stats[CreatureStat.ManaPoints].Maximum);

            ushort capacity = 1500; // Convert.ToUInt16(Math.Min(ushort.MaxValue, (ushort)playerStatsPacket.Player.Stats[CreatureStat.CarryStrength].Current));

            // ICombatant combatantPlayer = playerStatsPacket.Player as ICombatant;

            // Fail off by sending dummy data if the player for some reason is not a combatant.
            // Experience: 7.7x Client debugs after 0x7FFFFFFF (2,147,483,647) exp
            uint experience = 0; // combatantPlayer != null ? (uint)Math.Min(0x7FFFFFFF, Convert.ToUInt64(combatantPlayer.Skills[SkillType.Experience].CurrentCount)) : 0;
            ushort expLevel = 1; // (ushort)(combatantPlayer != null ? Math.Max(1, Math.Min(ushort.MaxValue, combatantPlayer.Skills[SkillType.Experience].CurrentLevel)) : 1);
            byte expPercentage = 0; // (byte)(combatantPlayer != null ? combatantPlayer.Skills[SkillType.Experience].Percent : 0);

            byte magicLevel = 0; // (byte)(combatantPlayer != null ? Math.Min(byte.MaxValue, combatantPlayer.Skills[SkillType.Magic].CurrentLevel) : 0);
            byte magicLevelPercentage = 0; // (byte)(combatantPlayer != null ? combatantPlayer.Skills[SkillType.Magic].Percent : 0);
            byte soulPoints = 0;

            message.AddByte(playerStatsPacket.PacketType.ToByte());

            message.AddUInt16(hitpoints);
            message.AddUInt16(maxHitpoints);
            message.AddUInt16(capacity);

            message.AddUInt32(experience);

            message.AddUInt16(expLevel);
            message.AddByte(expPercentage);
            message.AddUInt16(manapoints);
            message.AddUInt16(maxManapoints);
            message.AddByte(magicLevel);
            message.AddByte(magicLevelPercentage);

            message.AddByte(soulPoints);
        }
    }
}
