// -----------------------------------------------------------------
// <copyright file="ItemsContainerContentRemovedHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Delegates
{
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Delegate meant for when content is removed from a container.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="indexRemoved">The index of the item removed.</param>
    public delegate void ItemsContainerContentRemovedHandler(IItemsContainer container, byte indexRemoved);
}
