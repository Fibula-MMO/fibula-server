﻿// -----------------------------------------------------------------
// <copyright file="ItemsContainerContentUpdatedHandler.cs" company="2Dudes">
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
    /// Delegate meant for when content is updated in a container.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="updatedIndex">The index of the updated item.</param>
    /// <param name="updatedItem">The updated item.</param>
    public delegate void ItemsContainerContentUpdatedHandler(IItemsContainer container, byte updatedIndex, IItem updatedItem);
}
