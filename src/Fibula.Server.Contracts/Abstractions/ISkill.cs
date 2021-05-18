// -----------------------------------------------------------------
// <copyright file="ISkill.cs" company="2Dudes">
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
    using Fibula.Definitions.Enumerations;
    using Fibula.Server.Contracts.Delegates;

    /// <summary>
    /// Interface for skills in the game.
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// Event triggered when this skill level changes.
        /// </summary>
        event SkillLevelChangedHandler LevelChanged;

        /// <summary>
        /// Event triggered when this skill count changes.
        /// </summary>
        event SkillCountChangedHandler CountChanged;

        /// <summary>
        /// Event triggered when this skill percent changes.
        /// </summary>
        event SkillPercentChangedHandler PercentChanged;

        /// <summary>
        /// Gets this skill's type.
        /// </summary>
        SkillType Type { get; }

        /// <summary>
        /// Gets this skill's default level.
        /// </summary>
        uint DefaultLevel { get; }

        /// <summary>
        /// Gets this skill's current level.
        /// </summary>
        uint CurrentLevel { get; }

        /// <summary>
        /// Gets this skill's maximum level.
        /// </summary>
        uint MaximumLevel { get; }

        /// <summary>
        /// Gets the count at which the current level started.
        /// </summary>
        /// <remarks>
        /// Must be greater than <see cref="CountForNextLevel"/>.
        /// </remarks>
        double CountAtStartOfLevel { get; }

        /// <summary>
        /// Gets the count at which the current level will advance.
        /// </summary>
        /// <remarks>
        /// Must be greater than <see cref="CountAtStartOfLevel"/> to actually advance.
        /// </remarks>
        double CountForNextLevel { get; }

        /// <summary>
        /// Gets this skill's current count.
        /// </summary>
        double CurrentCount { get; }

        /// <summary>
        /// Gets the current percentual value between current and target counts this skill.
        /// </summary>
        byte Percent { get; }

        /// <summary>
        /// Increases this skill's counter.
        /// </summary>
        /// <param name="value">The amount by which to increase this skills counter.</param>
        void IncreaseCounter(double value);
    }
}
