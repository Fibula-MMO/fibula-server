// -----------------------------------------------------------------
// <copyright file="ContainerManagerClosedContainerHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Delegates
{
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Delegate for an event fired by an <see cref="IContainerManager"/> when it has closed a container.
    /// </summary>
    /// <param name="forPlayer">The player for which the container was closed.</param>
    /// <param name="atIndex">The index of the container that was closed.</param>
    public delegate void ContainerManagerClosedContainerHandler(IPlayer forPlayer, byte atIndex);
}
