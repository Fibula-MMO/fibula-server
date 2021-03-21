// -----------------------------------------------------------------
// <copyright file="IItemTypeReadOnlyRepository.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Contracts.Abstractions.Repositories
{
    using Fibula.Definitions.Data.Entities;

    /// <summary>
    /// Interface for a read-only repository of item types.
    /// </summary>
    public interface IItemTypeReadOnlyRepository : IReadOnlyRepository<ItemTypeEntity>
    {
        /// <summary>
        /// Gets an item type from the context.
        /// </summary>
        /// <param name="typeId">The id of the type to get.</param>
        /// <returns>The type found, if any.</returns>
        ItemTypeEntity GetByTypeId(string typeId);
    }
}
