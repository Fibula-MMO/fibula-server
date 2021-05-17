// -----------------------------------------------------------------
// <copyright file="ICreaturesContainer.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Abstractions
{
    /// <summary>
    /// Interface an entity that can contain <see cref="ICreature"/>s.
    /// </summary>
    public interface ICreaturesContainer : ILocatable
    {
        /// <summary>
        /// Attempts to add an <see cref="ICreature"/> to this container.
        /// </summary>
        /// <param name="creature">The creature to add to the container.</param>
        /// <returns>True if the creature was successfully added, and false otherwise.</returns>
        bool AddCreature(ICreature creature);

        /// <summary>
        /// Attempts to remove an <see cref="ICreature"/> from this container.
        /// </summary>
        /// <param name="creature">The creature to remove from the container.</param>
        /// <returns>True if the creature was successfully removed, and false otherwise.</returns>
        bool RemoveCreature(ICreature creature);
    }
}
