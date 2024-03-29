﻿// -----------------------------------------------------------------
// <copyright file="AutoMovePacketReader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Protocol.V772.PacketReaders
{
    using System.IO;
    using Fibula.Communications;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Communications.Packets.Incoming;
    using Fibula.Definitions.Enumerations;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents an auto walk packet reader for the game server.
    /// </summary>
    public sealed class AutoMovePacketReader : BasePacketReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMovePacketReader"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public AutoMovePacketReader(ILogger<AutoMovePacketReader> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Reads a packet from the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="message">The message to read from.</param>
        /// <returns>The packet read from the message.</returns>
        public override IInboundPacket ReadFromMessage(INetworkMessage message)
        {
            message.ThrowIfNull(nameof(message));

            var numberOfMovements = message.GetByte();

            var directions = new Direction[numberOfMovements];

            for (var i = 0; i < numberOfMovements; i++)
            {
                var dir = message.GetByte();

                directions[i] = dir switch
                {
                    1 => Direction.East,
                    2 => Direction.NorthEast,
                    3 => Direction.North,
                    4 => Direction.NorthWest,
                    5 => Direction.West,
                    6 => Direction.SouthWest,
                    7 => Direction.South,
                    8 => Direction.SouthEast,
                    _ => throw new InvalidDataException($"Invalid direction value '{dir}' on message."),
                };
            }

            return new AutoMovePacket(directions);
        }
    }
}
