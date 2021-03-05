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
    using System;

    /// <summary>
    /// Static class that contains common formulae used in the game.
    /// </summary>
    /// <remarks>
    /// Variables used in these formulae usually come from those in <see cref="ISkillProgressionFormulaInput"/>.
    /// </remarks>
    public static class ConstantFormulae
    {
        /// <summary>
        /// Gets the delegate used to calculate the next target count for experience.
        /// </summary>
        public static Func<ISkillProgressionFormulaInput, double> ExperienceNextTargetCountDelegate => (input) => ((50 * Math.Pow(input.CurrentLevel, 3)) - (150 * Math.Pow(input.CurrentLevel, 2)) + (400 * input.CurrentLevel)) / 3;

        /// <summary>
        /// Gets the default delegate used to calculate the next target count for skills.
        /// </summary>
        public static Func<ISkillProgressionFormulaInput, double> DefaultSkillNextTargetCountDelegate => (input) => 50 * (1 - Math.Pow(1.1, 1 + input.CurrentLevel - 10)) / (1 - 1.1);
    }
}
