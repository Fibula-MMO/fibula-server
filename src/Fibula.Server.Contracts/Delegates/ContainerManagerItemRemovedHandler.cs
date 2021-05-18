// -----------------------------------------------------------------
// <copyright file="ContainerManagerItemRemovedHandler.cs" company="2Dudes">
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
    /// Delegate for an event fired by an <see cref="IContainerManager"/> when an item is removed from a container.
    /// </summary>
    /// <param name="forPlayer">The player that will be notified of the event.</param>
    /// <param name="fromContainerId">The id of the container from which the item is removed.</param>
    /// <param name="atIndex">The index from which the item was removed.</param>
    public delegate void ContainerManagerItemRemovedHandler(IPlayer forPlayer, byte fromContainerId, byte atIndex);
}
