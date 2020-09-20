// -----------------------------------------------------------------
// <copyright file="SkillProgressionFormulaInput.cs" company="2Dudes">
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
    /// Class that represents the input for a skill's progression formula.
    /// </summary>
    public class SkillProgressionFormulaInput : ISkillProgressionFormulaInput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillProgressionFormulaInput"/> class.
        /// </summary>
        /// <param name="skillType">The type of skill.</param>
        /// <param name="currentSkillLevel">The current skill level.</param>
        /// <param name="professionType">The current profession.</param>
        public SkillProgressionFormulaInput(SkillType skillType, uint currentSkillLevel, ProfessionType professionType)
        {
            this.SkillType = skillType;
            this.CurrentLevel = currentSkillLevel;
            this.Profession = professionType;
        }

        /// <summary>
        /// Gets the type of skill.
        /// </summary>
        public SkillType SkillType { get; }

        /// <summary>
        /// Gets the current skill level.
        /// </summary>
        public uint CurrentLevel { get; }

        /// <summary>
        /// Gets the current profession of the creature.
        /// </summary>
        public ProfessionType Profession { get; }
    }
}
