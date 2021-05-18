// -----------------------------------------------------------------
// <copyright file="IThingsContainer.cs" company="2Dudes">
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
    /// Interface an entity that can contain both <see cref="IItem"/>s and <see cref="ICreature"/>s.
    /// </summary>
    public interface IThingsContainer : IItemsContainer, ICreaturesContainer
    {
        /// <summary>
        /// Attempts to find a <see cref="IThing"/> in the container, at the given index.
        /// </summary>
        /// <param name="index">The index at which to look for.</param>
        /// <returns>The <see cref="IThing"/> found at the index, and null otherwise.</returns>
        new IThing this[int index] { get; }

        /// <summary>
        /// Attempts to get the index for the given <see cref="IThing"/> within this container.
        /// </summary>
        /// <param name="thing">The thing to find.</param>
        /// <returns>The index for the <see cref="IThing"/> found, or <see cref="byte.MaxValue"/> if it was not found.</returns>
        byte GetIndexOfThing(IThing thing);
    }
}
