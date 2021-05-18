// -----------------------------------------------------------------
// <copyright file="PlayerCreationArguments.cs" company="2Dudes">
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
    using Fibula.Definitions.Data.Entities;

    /// <summary>
    /// Class that represents creation arguments for players.
    /// </summary>
    public class PlayerCreationArguments : CreatureCreationArguments
    {
        /// <summary>
        /// Gets the character's metadata.
        /// </summary>
        public CharacterEntity CharacterMetadata => this.Metadata as CharacterEntity;
    }
}
