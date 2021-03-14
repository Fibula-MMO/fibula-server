// -----------------------------------------------------------------
// <copyright file="IMonsterTypesLoader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Entities.Contracts.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for an <see cref="IMonsterTypesLoader"/> loader.
    /// </summary>
    public interface IMonsterTypesLoader
    {
        /// <summary>
        /// Attempts to load the monster catalog.
        /// </summary>
        /// <returns>The catalog, containing a mapping of loaded id to the monster types.</returns>
        IDictionary<string, MonsterTypeEntity> LoadTypes();
    }
}
