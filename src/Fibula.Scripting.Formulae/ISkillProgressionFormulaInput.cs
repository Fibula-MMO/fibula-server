// -----------------------------------------------------------------
// <copyright file="ISkillProgressionFormulaInput.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Scripting.Formulae
{
    using Fibula.Data.Entities.Contracts.Enumerations;

    /// <summary>
    /// Interface for the input used on the skill progression formula.
    /// </summary>
    public interface ISkillProgressionFormulaInput
    {
        /// <summary>
        /// Gets the type of skill.
        /// </summary>
        SkillType SkillType { get; }

        /// <summary>
        /// Gets the current skill level.
        /// </summary>
        uint CurrentLevel { get; }

        /// <summary>
        /// Gets the current profession of the creature.
        /// </summary>
        ProfessionType Profession { get; }
    }
}
