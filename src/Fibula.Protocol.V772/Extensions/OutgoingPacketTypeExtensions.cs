// -----------------------------------------------------------------
// <copyright file="OutgoingPacketTypeExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772.Extensions
{
    using System;
    using Fibula.Communications.Packets.Contracts.Enumerations;

    /// <summary>
    /// Static class that contains helper methods to convert <see cref="OutboundPacketType"/>.
    /// </summary>
    public static class OutgoingPacketTypeExtensions
    {
        /// <summary>
        /// Attempts to convert an <see cref="OutboundPacketType"/> value into a byte value.
        /// </summary>
        /// <param name="packetType">The packet type to convert.</param>
        /// <returns>The byte value converted to.</returns>
        public static byte ToByte(this OutboundPacketType packetType)
        {
            return packetType switch
            {
                OutboundPacketType.GatewayDisconnect => 0x0A,
                OutboundPacketType.MessageOfTheDay => 0x14,
                OutboundPacketType.CharacterList => 0x64,
                OutboundPacketType.PlayerLogin => 0x0A,
                OutboundPacketType.GamemasterFlags => 0x0B,
                OutboundPacketType.GameDisconnect => 0x14,
                OutboundPacketType.WaitingList => 0x16,
                OutboundPacketType.HeartbeatResponse => 0x1D,
                OutboundPacketType.Heartbeat => 0x1E,
                OutboundPacketType.Death => 0x28,
                OutboundPacketType.AddUnknownCreature => 0x61,
                OutboundPacketType.AddKnownCreature => 0x62,
                OutboundPacketType.MapDescription => 0x64,
                OutboundPacketType.MapSliceNorth => 0x65,
                OutboundPacketType.MapSliceEast => 0x66,
                OutboundPacketType.MapSliceSouth => 0x67,
                OutboundPacketType.MapSliceWest => 0x68,
                OutboundPacketType.TileUpdate => 0x69,
                OutboundPacketType.AddThing => 0x6A,
                OutboundPacketType.UpdateThing => 0x6B,
                OutboundPacketType.RemoveThing => 0x6C,
                OutboundPacketType.CreatureMoved => 0x6D,
                OutboundPacketType.ContainerOpen => 0x6E,
                OutboundPacketType.ContainerClose => 0x6F,
                OutboundPacketType.ContainerAddItem => 0x70,
                OutboundPacketType.ContainerUpdateItem => 0x71,
                OutboundPacketType.ContainerRemoveItem => 0x72,
                OutboundPacketType.InventoryItem => 0x78,
                OutboundPacketType.InventoryEmpty => 0x79,
                OutboundPacketType.WorldLight => 0x82,
                OutboundPacketType.MagicEffect => 0x83,
                OutboundPacketType.AnimatedText => 0x84,
                OutboundPacketType.ProjectileEffect => 0x85,
                OutboundPacketType.Square => 0x86,
                OutboundPacketType.CreatureHealth => 0x8C,
                OutboundPacketType.CreatureLight => 0x8D,
                OutboundPacketType.CreatureOutfit => 0x8E,
                OutboundPacketType.CreatureSpeedChange => 0x8F,
                OutboundPacketType.CreatureSkull => 0x90,
                OutboundPacketType.CreatureShield => 0x91,
                OutboundPacketType.TextWindow => 0x96,
                OutboundPacketType.HouseWindow => 0x97,
                OutboundPacketType.PlayerStats => 0xA0,
                OutboundPacketType.PlayerSkills => 0xA1,
                OutboundPacketType.PlayerConditions => 0xA2,
                OutboundPacketType.CancelAttack => 0xA3,
                OutboundPacketType.PlayerModes => 0xA7,
                OutboundPacketType.CreatureSpeech => 0xAA,
                OutboundPacketType.TextMessage => 0xB4,
                OutboundPacketType.CancelWalk => 0xB5,
                OutboundPacketType.FloorChangeUp => 0xBE,
                OutboundPacketType.FloorChangeDown => 0xBF,
                OutboundPacketType.OutfitWindow => 0xC8,
                OutboundPacketType.VipDetails => 0xD2,
                OutboundPacketType.VipOnline => 0xD3,
                OutboundPacketType.VipOffline => 0xD4,
                _ => throw new NotSupportedException($"Outgoing packet type {packetType} is not supported in this client version.")
            };
        }
    }
}
