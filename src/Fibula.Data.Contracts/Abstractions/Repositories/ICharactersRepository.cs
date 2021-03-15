// -----------------------------------------------------------------
// <copyright file="ICharactersRepository.cs" company="2Dudes">
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
    /// Interface for a repository of accounts.
    /// </summary>
    public interface ICharactersRepository
    {
        /// <summary>
        /// Attempts to find a character in the repo with a given name.
        /// </summary>
        /// <param name="characterName">The name of the character.</param>
        /// <returns>The entity if one was bound, and null otherwise.</returns>
        CharacterEntity FindCharacterByName(string characterName);
    }
}
