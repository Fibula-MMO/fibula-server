// -----------------------------------------------------------------
// <copyright file="IInventory.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Abstractions
{
    /// <summary>
    /// Interface for an inventory of <see cref="IItem"/>s, and the properties it imbues the owner <see cref="ICreature"/> with.
    /// </summary>
    public interface IInventory : IContainerItem
    {
        /// <summary>
        /// Gets a reference to the owner of this inventory.
        /// </summary>
        ICreature Owner { get; }

        // TODO: add special properties given by items here.
    }
}
