// -----------------------------------------------------------------
// <copyright file="IContainerItem.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for <see cref="IItem"/>s that are containers for other <see cref="IItem"/>s.
    /// </summary>
    public interface IContainerItem : IItem, IItemsContainer
    {
        /// <summary>
        /// Gets the collection of items contained in this container.
        /// </summary>
        IList<IItem> Content { get; }

        /// <summary>
        /// Gets the capacity of this container.
        /// </summary>
        byte Capacity { get; }

        /// <summary>
        /// Evaluates whether this item is descendant of a given item.
        /// </summary>
        /// <param name="itemsContainer">The item to check.</param>
        /// <returns>True if this item is child of any item in the parent hierarchy, false otherwise.</returns>
        bool IsDescendantOf(IItemsContainer itemsContainer);
    }
}
