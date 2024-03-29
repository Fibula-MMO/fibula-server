﻿// -----------------------------------------------------------------
// <copyright file="IPredefinedItemSet.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Interface that defines pre-defined items that are used by the server logic.
    /// </summary>
    public interface IPredefinedItemSet
    {
        /// <summary>
        /// Finds the splatter <see cref="ItemTypeEntity"/> for a given blood type.
        /// </summary>
        /// <param name="bloodType">The type of blood to look the item type for.</param>
        /// <returns>The <see cref="ItemTypeEntity"/> that's predefined for that blood type, or null if none is.</returns>
        ItemTypeEntity FindSplatterForBloodType(BloodType bloodType);

        /// <summary>
        /// Finds the pool <see cref="ItemTypeEntity"/> for a given blood type.
        /// </summary>
        /// <param name="bloodType">The type of blood to look the item type for.</param>
        /// <returns>The <see cref="ItemTypeEntity"/> that's predefined for that blood type, or null if none is.</returns>
        ItemTypeEntity FindPoolForBloodType(BloodType bloodType);
    }
}
