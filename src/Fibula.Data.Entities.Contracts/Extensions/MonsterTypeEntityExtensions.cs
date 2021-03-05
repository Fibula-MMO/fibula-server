// -----------------------------------------------------------------
// <copyright file="MonsterTypeEntityExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Entities.Contracts.Extensions
{
    using Fibula.Data.Entities.Contracts.Abstractions;
    using Fibula.Definitions.Flags;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Static class that contains extension methods for <see cref="IMonsterTypeEntity"/>s.
    /// </summary>
    public static class MonsterTypeEntityExtensions
    {
        /// <summary>
        /// Checks if the monster type has the given creature flag set.
        /// </summary>
        /// <param name="monsterTypeEntity">The monster type entity.</param>
        /// <param name="creatureFlag">The creature flag to check for.</param>
        /// <returns>True if the monster type has the creature flag set, and false otherwise.</returns>
        public static bool HasCreatureFlag(this IMonsterTypeEntity monsterTypeEntity, CreatureFlag creatureFlag)
        {
            monsterTypeEntity.ThrowIfNull(nameof(monsterTypeEntity));

            return (monsterTypeEntity.Flags & (ulong)creatureFlag) == (ulong)creatureFlag;
        }
    }
}
