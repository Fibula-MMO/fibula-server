// -----------------------------------------------------------------
// <copyright file="ConstantFormulae.cs" company="2Dudes">
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
    /// <summary>
    /// Static class that contains common formulae used in the game.
    /// </summary>
    /// <remarks>
    /// Variables used in these formulae usually come from those in <see cref="ISkillProgressionFormulaInput"/>.
    /// </remarks>
    public static class ConstantFormulae
    {
        /// <summary>
        /// The formula used to calculate the next target count for experience.
        /// </summary>
        public const string ExperienceNextTargetCountFormula = "return (50 * System.Math.Pow(CurrentLevel, 3) - 150 * System.Math.Pow(CurrentLevel, 2) + 400 * CurrentLevel) / 3;";

        /// <summary>
        /// The default formula used to calculate the next target count for skills.
        /// </summary>
        public const string DefaultSkillNextTargetCountFormula = "return 50 * (1 - System.Math.Pow(1.1, 1 + CurrentLevel - 10)) / (1 - 1.1);";
    }
}
