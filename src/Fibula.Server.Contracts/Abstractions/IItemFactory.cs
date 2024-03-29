﻿// -----------------------------------------------------------------
// <copyright file="IItemFactory.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Entities;
    using Fibula.Server.Contracts.Delegates;

    /// <summary>
    /// Interface for an item factory.
    /// </summary>
    public interface IItemFactory
    {
        /// <summary>
        /// Event called when an item is created.
        /// </summary>
        event ItemFactoryItemCreatedHandler ItemCreated;

        /// <summary>
        /// Creates a new <see cref="IItem"/>.
        /// </summary>
        /// <param name="creationArguments">The arguments for the <see cref="IItem"/> creation.</param>
        /// <returns>A new instance of the <see cref="IItem"/>.</returns>
        IItem CreateItem(IItemCreationArguments creationArguments);

        /// <summary>
        /// Looks up an <see cref="ItemTypeEntity"/> given a type id.
        /// </summary>
        /// <param name="typeId">The id of the type to look for.</param>
        /// <returns>A reference to the <see cref="ItemTypeEntity"/> found, and null if not found.</returns>
        ItemTypeEntity FindTypeById(ushort typeId);
    }
}
