// -----------------------------------------------------------------
// <copyright file="CharacterRepository.cs" company="2Dudes">
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
    using Fibula.Data.Contracts.Abstractions.Repositories;
    using Fibula.Definitions.Data.Entities;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Class that represents a character repository.
    /// </summary>
    public class CharacterRepository : GenericRepository<CharacterEntity>, ICharactersRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterRepository"/> class.
        /// </summary>
        /// <param name="context">The context to initialize the repository with.</param>
        public CharacterRepository(DbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Attempts to find a character in the repo with a given name.
        /// </summary>
        /// <param name="characterName">The name of the character.</param>
        /// <returns>The entity if one was bound, and null otherwise.</returns>
        /// <remarks>Does not load nested relationships, only those at this entity level.</remarks>
        public CharacterEntity FindCharacterByName(string characterName)
        {
            return this.FindOne(c => c.Name.ToUpper() == characterName.ToUpper(), nameof(CharacterEntity.Stats));
        }
    }
}
