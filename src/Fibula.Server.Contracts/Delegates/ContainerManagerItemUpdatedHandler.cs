// -----------------------------------------------------------------
// <copyright file="ContainerManagerItemUpdatedHandler.cs" company="2Dudes">
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
    /// Delegate for an event fired by an <see cref="IContainerManager"/> when an item is updated in a container.
    /// </summary>
    /// <param name="forPlayer">The player that will be notified of the event.</param>
    /// <param name="containerId">The id of the container in which the item was updated.</param>
    /// <param name="item">The item that was updated.</param>
    public delegate void ContainerManagerItemUpdatedHandler(IPlayer forPlayer, byte containerId, IItem item);
}
