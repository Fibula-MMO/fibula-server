﻿// -----------------------------------------------------------------
// <copyright file="ICreatureFactory.cs" company="2Dudes">
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
    /// <summary>
    /// Interface for an <see cref="ICreature"/> factory.
    /// </summary>
    public interface ICreatureFactory
    {
        ///// <summary>
        ///// Event called when a creature is created.
        ///// </summary>
        // event OnCreatureCreated CreatureCreated;

        /// <summary>
        /// Creates a new implementation instance of <see cref="ICreature"/> depending on the chosen type.
        /// </summary>
        /// <param name="creationArguments">The creation arguments for the new creature.</param>
        /// <returns>A new instance of the chosen <see cref="ICreature"/> implementation.</returns>
        ICreature CreateCreature(ICreatureCreationArguments creationArguments);
    }
}
