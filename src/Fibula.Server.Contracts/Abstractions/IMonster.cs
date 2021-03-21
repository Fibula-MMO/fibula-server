// -----------------------------------------------------------------
// <copyright file="IMonster.cs" company="2Dudes">
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

    /// <summary>
    /// Interface for all monsters.
    /// </summary>
    public interface IMonster : ICreature
    {
        /// <summary>
        /// Gets the type of this monster.
        /// </summary>
        MonsterTypeEntity Type { get; }

        /// <summary>
        /// Gets the experience yielded when this monster dies.
        /// </summary>
        uint ExperienceToYield { get; }
    }
}
