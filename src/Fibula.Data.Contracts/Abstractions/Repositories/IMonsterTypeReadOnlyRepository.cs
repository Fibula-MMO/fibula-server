// -----------------------------------------------------------------
// <copyright file="IMonsterTypeReadOnlyRepository.cs" company="2Dudes">
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
    /// Interface for a read-only repository of monster types.
    /// </summary>
    public interface IMonsterTypeReadOnlyRepository : IReadOnlyRepository<MonsterTypeEntity>
    {
        /// <summary>
        /// Gets a monster type from the context.
        /// </summary>
        /// <param name="raceId">The id of the race of monster type to get.</param>
        /// <returns>The monster type found, if any.</returns>
        MonsterTypeEntity GetByRaceId(string raceId);
    }
}
