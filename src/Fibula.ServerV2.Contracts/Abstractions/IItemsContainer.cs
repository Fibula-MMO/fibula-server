// -----------------------------------------------------------------
// <copyright file="IItemsContainer.cs" company="2Dudes">
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
    /// <summary>
    /// Interface an entity that can contain <see cref="IItem"/>s.
    /// </summary>
    public interface IItemsContainer : ILocatable
    {
        /// <summary>
        /// Attempts to add an <see cref="IItem"/> to this container.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be added.</param>
        /// <param name="itemToAdd">The item to add to the container.</param>
        /// <param name="atIndex">Optional. The index at which to add the item. Defaults to a value of <see cref="byte.MaxValue"/>, which means to try adding at any free index.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the item may be returned.</returns>
        (bool result, IItem remainder) AddItem(IItemFactory itemFactory, IItem itemToAdd, byte atIndex = byte.MaxValue);

        /// <summary>
        /// Attempts to remove an <see cref="IItem"/> from this container.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be removed, like when a smaller <paramref name="amount"/> than what is available is specified.</param>
        /// <param name="itemToRemove">The item to remove from the container.</param>
        /// <param name="amount">Optional. The amount of the <paramref name="itemToRemove"/> to remove.</param>
        /// <param name="index">Optional. The index from which to remove the item. Defaults to <see cref="byte.MaxValue"/>, which means to try removing the item at any index that it's found.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the item may be returned.</returns>
        (bool result, IItem remainder) RemoveItem(IItemFactory itemFactory, ref IItem itemToRemove, byte amount = 1, byte index = byte.MaxValue);

        /// <summary>
        /// Attempts to replace an <see cref="IItem"/> from this container by removing the <paramref name="itemToRemove"/> and adding the <paramref name="itemToAdd"/>.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be removed, like when a smaller <paramref name="amount"/> than what is available is specified.</param>
        /// <param name="itemToRemove">The item to remove from the container.</param>
        /// <param name="itemToAdd">The item to add to the container.</param>
        /// <param name="amount">Optional. The amount of the <paramref name="itemToRemove"/> to replace.</param>
        /// <param name="index">Optional. The index from which to remove the item. Defaults to <see cref="byte.MaxValue"/>, which means to try removing the item at any index that it's found.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the thing may be returned.</returns>
        (bool result, IItem remainderToChange) ReplaceItem(IItemFactory itemFactory, IItem itemToRemove, IItem itemToAdd, byte amount = 1, byte index = byte.MaxValue);

        /// <summary>
        /// Attempts to find an <see cref="IItem"/> in this container, at a given index.
        /// </summary>
        /// <param name="index">The index at which to look.</param>
        /// <returns>The <see cref="IItem"/> found at the index, if any was found, and null otherwise.</returns>
        IItem FindItemAtIndex(byte index);

        /// <summary>
        /// Attempts to find an <see cref="IItem"/> in this container, of a given type.
        /// </summary>
        /// <param name="typeId">The id of the type to look for.</param>
        /// <returns>The item found, if any, and null otherwise.</returns>
        IItem FindItemByTypeId(ushort typeId);
    }
}
