// -----------------------------------------------------------------
// <copyright file="PlayerInventoryClearSlotPacket.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Packets.Outgoing
{
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Enumerations;
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Class that represents a player's clear inventory slot packet.
    /// </summary>
    public class PlayerInventoryClearSlotPacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerInventoryClearSlotPacket"/> class.
        /// </summary>
        /// <param name="slot">The slot that this packet is about.</param>
        public PlayerInventoryClearSlotPacket(Slot slot)
        {
            this.Slot = slot;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutboundPacketType PacketType => OutboundPacketType.InventoryEmpty;

        /// <summary>
        /// Gets the slot.
        /// </summary>
        public Slot Slot { get; }
    }
}
