// -----------------------------------------------------------------
// <copyright file="SkillCountChangedHandler.cs" company="2Dudes">
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
    /// Delegate meant for a skill count change.
    /// </summary>
    /// <param name="skillType">The type of skill that changed.</param>
    /// <param name="countDelta">The delta in the count for this skill.</param>
    public delegate void SkillCountChangedHandler(SkillType skillType, long countDelta);
}
