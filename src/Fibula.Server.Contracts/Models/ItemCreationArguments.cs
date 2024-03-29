﻿// -----------------------------------------------------------------
// <copyright file="ItemCreationArguments.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Models
{
    using System;
    using System.Collections.Generic;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Abstractions;

    /// <summary>
    /// Class that implements <see cref="IItemCreationArguments"/>, for the creation of an item.
    /// </summary>
    public class ItemCreationArguments : IItemCreationArguments
    {
        /// <summary>
        /// Gets or sets the id of type for the item to create.
        /// </summary>
        public ushort TypeId { get; set; }

        /// <summary>
        /// Gets or sets the attributes to set in the item to create.
        /// </summary>
        public IReadOnlyCollection<(ItemAttribute, IConvertible)> Attributes { get; set; }

        /// <summary>
        /// Convenient method that initializes <see cref="ItemCreationArguments"/> with the given type id.
        /// </summary>
        /// <param name="typeId">The type id of the item to create.</param>
        /// <returns>A new instance of <see cref="ItemCreationArguments"/> specifying the given type id.</returns>
        public static ItemCreationArguments WithTypeId(ushort typeId) => new () { TypeId = typeId };
    }
}
