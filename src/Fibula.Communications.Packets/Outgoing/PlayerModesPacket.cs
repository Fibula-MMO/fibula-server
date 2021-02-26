﻿// -----------------------------------------------------------------
// <copyright file="PlayerModesPacket.cs" company="2Dudes">
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
    using Fibula.Common.Contracts.Enumerations;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Contracts.Enumerations;

    /// <summary>
    /// Class that represents a player's modes packet.
    /// </summary>
    public class PlayerModesPacket : IOutboundPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerModesPacket"/> class.
        /// </summary>
        /// <param name="chaseMode">The chase mode.</param>
        public PlayerModesPacket(ChaseMode chaseMode)
        {
            this.ChaseMode = chaseMode;
        }

        /// <summary>
        /// Gets the type of this packet.
        /// </summary>
        public OutgoingPacketType PacketType => OutgoingPacketType.PlayerModes;

        /// <summary>
        /// Gets the chase mode to set.
        /// </summary>
        public ChaseMode ChaseMode { get; }
    }
}
