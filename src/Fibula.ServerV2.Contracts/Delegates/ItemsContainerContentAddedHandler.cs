// -----------------------------------------------------------------
// <copyright file="ItemsContainerContentAddedHandler.cs" company="2Dudes">
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
    /// Delegate meant for when content is added to a container.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="addedItem">The added item.</param>
    public delegate void ItemsContainerContentAddedHandler(IItemsContainer container, IItem addedItem);
}
