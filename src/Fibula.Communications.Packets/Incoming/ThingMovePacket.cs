﻿// -----------------------------------------------------------------
// <copyright file="ThingMovePacket.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Communications.Packets.Incoming
{
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Definitions.Data.Structures;

    /// <summary>
    /// Class that represents a thing movement packet.
    /// </summary>
    public class ThingMovePacket : IInboundPacket, IThingMoveInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingMovePacket"/> class.
        /// </summary>
        /// <param name="fromLocation">The location from which the thing is being moved.</param>
        /// <param name="thingClientId">The id of the thing being moved.</param>
        /// <param name="fromStackPos">The position in the stack of the thing being moved.</param>
        /// <param name="toLocation">The location to which the thing is being moved.</param>
        /// <param name="count">The amount of the thing being moved.</param>
        public ThingMovePacket(Location fromLocation, ushort thingClientId, byte fromStackPos, Location toLocation, byte count)
        {
            this.ThingClientId = thingClientId;

            this.FromLocation = fromLocation;
            this.FromStackPos = fromStackPos;

            this.ToLocation = toLocation;

            this.Amount = count;
        }

        /// <summary>
        /// Gets the id of the thing, as seen by the client.
        /// </summary>
        public ushort ThingClientId { get; }

        /// <summary>
        /// Gets the location from which the thing is being moved.
        /// </summary>
        public Location FromLocation { get; }

        /// <summary>
        /// Gets the position in the stack at the location from which the thing is being moved.
        /// </summary>
        public byte FromStackPos { get; }

        /// <summary>
        /// Gets the location to which the thing is being moved.
        /// </summary>
        public Location ToLocation { get; }

        /// <summary>
        /// Gets the amount of the thing being moved.
        /// </summary>
        public byte Amount { get; }
    }
}
