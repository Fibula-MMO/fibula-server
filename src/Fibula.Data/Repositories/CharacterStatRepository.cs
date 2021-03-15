// -----------------------------------------------------------------
// <copyright file="CharacterStatRepository.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Repositories
{
    using Fibula.Definitions.Data.Entities;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Class that represents a repository for character stats.
    /// </summary>
    public class CharacterStatRepository : GenericRepository<CharacterStatEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterStatRepository"/> class.
        /// </summary>
        /// <param name="context">The context to initialize the repository with.</param>
        public CharacterStatRepository(DbContext context)
            : base(context)
        {
        }
    }
}
