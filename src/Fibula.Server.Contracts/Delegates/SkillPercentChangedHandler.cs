// -----------------------------------------------------------------
// <copyright file="SkillPercentChangedHandler.cs" company="2Dudes">
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
    /// Delegate meant for a skill change in percentual value.
    /// </summary>
    /// <param name="skillType">The type of skill that changed.</param>
    /// <param name="previousPercent">The previous percent of completion to next level.</param>
    public delegate void SkillPercentChangedHandler(SkillType skillType, byte previousPercent);
}
