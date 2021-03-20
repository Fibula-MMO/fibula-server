// -----------------------------------------------------------------
// <copyright file="LookAtHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Mechanics.Handlers
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Fibula.Communications.Packets.Contracts.Abstractions;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a look at handler for the game server.
    /// </summary>
    public sealed class LookAtHandler : GameHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookAtHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="gameInstance">A reference to the game instance.</param>
        /// <param name="creatureFinder">A reference to the creature finder in use.</param>
        public LookAtHandler(ILogger<LookAtHandler> logger, IGame gameInstance, ICreatureFinder creatureFinder)
            : base(logger, gameInstance)
        {
            this.CreatureFinder = creatureFinder;
        }

        /// <summary>
        /// Gets the creature finder to use.
        /// </summary>
        public ICreatureFinder CreatureFinder { get; }

        /// <summary>
        /// Handles the contents of a network message.
        /// </summary>
        /// <param name="incomingPacket">The packet to handle.</param>
        /// <param name="client">A reference to the client from where this request originated from, for context.</param>
        /// <returns>A collection of <see cref="IOutboundPacket"/>s that compose that synchronous response, if any.</returns>
        public override IEnumerable<IOutboundPacket> HandleRequest(IIncomingPacket incomingPacket, IClient client)
        {
            incomingPacket.ThrowIfNull(nameof(incomingPacket));
            client.ThrowIfNull(nameof(client));

            if (!(incomingPacket is ILookAtInfo lookAtInfo))
            {
                this.Logger.LogError($"Expected packet info of type {nameof(ILookAtInfo)} but got {incomingPacket.GetType().Name}.");

                return null;
            }

            if (!(this.CreatureFinder.FindCreatureById(client.PlayerId) is IPlayer player))
            {
                this.Logger.LogWarning($"Client's associated player could not be found. [Id={client.PlayerId}]");

                return null;
            }

            this.Game.LookAt(lookAtInfo.ThingId, lookAtInfo.Location, lookAtInfo.StackPosition, player);

            return null;
        }
    }
}
