// -----------------------------------------------------------------
// <copyright file="ContainerManagerOpenedContainerHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.TcpServer.Contracts.Delegates
{
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate for an event fired by an <see cref="IContainerManager"/> when it has opened a container.
    /// </summary>
    /// <param name="forPlayer">The player for which the container was opened.</param>
    /// <param name="atIndex">The index at which the container was opened.</param>
    /// <param name="container">The container.</param>
    public delegate void ContainerManagerOpenedContainerHandler(IPlayer forPlayer, byte atIndex, IContainerItem container);
}
