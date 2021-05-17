// -----------------------------------------------------------------
// <copyright file="AddCreaturePacketWriter.cs" company="2Dudes">
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
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents an add creature packet writer for the game server.
    /// </summary>
    public class AddCreaturePacketWriter : BasePacketWriter
    {
        private readonly IClientsManager clientsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddCreaturePacketWriter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="clientsManager">A reference to the manager of clients.</param>
        public AddCreaturePacketWriter(ILogger<AddCreaturePacketWriter> logger, IClientsManager clientsManager)
            : base(logger)
        {
            clientsManager.ThrowIfNull(nameof(clientsManager));

            this.clientsManager = clientsManager;
        }

        /// <summary>
        /// Writes a packet to the given <see cref="INetworkMessage"/>.
        /// </summary>
        /// <param name="packet">The packet to write.</param>
        /// <param name="message">The message to write into.</param>
        public override void WriteToMessage(IOutboundPacket packet, ref INetworkMessage message)
        {
            if (!(packet is AddCreaturePacket addCreaturePacket))
            {
                this.Logger.LogWarning($"Invalid packet {packet.GetType().Name} routed to {this.GetType().Name}");

                return;
            }

            var client = this.clientsManager.FindByPlayerId(addCreaturePacket.Player.Id);
            var isCreatureKnown = client != null && client.KnowsCreatureWithId(addCreaturePacket.Creature.Id);
            var creatureIdToForget = client == null || isCreatureKnown ? 0 : client.ChooseCreatureToRemoveFromKnownSet();

            if (creatureIdToForget > 0)
            {
                client.RemoveKnownCreature(creatureIdToForget);
            }

            message.AddByte(addCreaturePacket.PacketType.ToByte());

            message.AddLocation(addCreaturePacket.Creature.Location);
            message.AddCreature(addCreaturePacket.Creature, isCreatureKnown, creatureIdToForget);
        }
    }
}
