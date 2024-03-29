﻿// -----------------------------------------------------------------
// <copyright file="ContainerMoveUpHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics.Handlers
{
    using System.Collections.Generic;
    using Fibula.Communications.Contracts.Abstractions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a handler for a player clicking the move up arrow in a container.
    /// </summary>
    public class ContainerMoveUpHandler : BaseTcpRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerMoveUpHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="gameContext">A reference to the game context to use.</param>
        public ContainerMoveUpHandler(ILogger<ContainerMoveUpHandler> logger, IGameContext gameContext)
            : base(logger, gameContext)
        {
        }

        /// <summary>
        /// Gets the type of packet that this handler is for.
        /// </summary>
        public override byte ForRequestType => (byte)GameRequestType.ContainerUp;

        /// <summary>
        /// Handles the contents of a network message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <param name="connection">A reference to the connection from where this message is comming from, for context.</param>
        /// <returns>A collection of <see cref="IResponsePacket"/>s that compose that synchronous response, if any.</returns>
        public override IEnumerable<IResponsePacket> HandleRequest(INetworkMessage message, IConnection connection)
        {
            var containerInfo = message.ReadContainerMoveUpInfo();

            if (this.Context.CreatureFinder.FindCreatureById(connection.PlayerId) is IPlayer player)
            {
                var container = this.Context.ContainerManager.FindForCreature(player.Id, containerInfo.ContainerId);

                if (container != null && container.ParentCylinder is IContainerItem)
                {
                    this.ScheduleNewOperation(
                        this.Context.OperationFactory.Create(
                            OperationType.ContainerMoveUp,
                            new MoveUpContainerOperationCreationArguments(player.Id, player, container, containerInfo.ContainerId)));
                }
            }

            return null;
        }
    }
}