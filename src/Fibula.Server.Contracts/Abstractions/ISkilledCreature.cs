// -----------------------------------------------------------------
// <copyright file="ISkilledCreature.cs" company="2Dudes">
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
    using System.Collections.Generic;
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Delegates;

    /// <summary>
    /// Interface for any creature in the game that has skills.
    /// </summary>
    public interface ISkilledCreature : ICreature
    {
        /// <summary>
        /// Event triggered when this skilled creature's skill changed.
        /// </summary>
        event SkilledCreatureSkillChangedHandler SkillChanged;

        /// <summary>
        /// Gets the current skills information for the entity.
        /// </summary>
        /// <remarks>
        /// The key is a <see cref="SkillType"/>, and the value is a <see cref="ISkill"/>.
        /// </remarks>
        IDictionary<SkillType, ISkill> Skills { get; }
    }
}
