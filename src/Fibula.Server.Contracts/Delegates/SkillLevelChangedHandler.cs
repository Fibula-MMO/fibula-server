// -----------------------------------------------------------------
// <copyright file="SkillLevelChangedHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Delegates
{
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Delegate meant for a skill level change.
    /// </summary>
    /// <param name="skillType">The type of skill that changed.</param>
    /// <param name="previousLevel">The previous skill level.</param>
    public delegate void SkillLevelChangedHandler(SkillType skillType, uint previousLevel);
}
