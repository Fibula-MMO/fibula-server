﻿// -----------------------------------------------------------------
// <copyright file="CreatureFactory.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Creatures
{
    using System;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Contracts.Models;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a factory of creatures.
    /// </summary>
    public class CreatureFactory : ICreatureFactory
    {
        /// <summary>
        /// The application wide context.
        /// </summary>
        private readonly IApplicationContext applicationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureFactory"/> class.
        /// </summary>
        /// <param name="applicationContext">A reference to the application context.</param>
        public CreatureFactory(IApplicationContext applicationContext)
        {
            applicationContext.ThrowIfNull(nameof(applicationContext));

            this.applicationContext = applicationContext;
        }

        /// <summary>
        /// Creates a new implementation instance of <see cref="ICreature"/> depending on the chosen type.
        /// </summary>
        /// <param name="creatureCreationArguments">The arguments for the creature creation.</param>
        /// <returns>A new instance of the chosen <see cref="ICreature"/> implementation.</returns>
        public ICreature CreateCreature(ICreatureCreationArguments creatureCreationArguments)
        {
            creatureCreationArguments.ThrowIfNull(nameof(creatureCreationArguments));

            switch (creatureCreationArguments.Type)
            {
                // TODO: suppport other types
                // case CreatureType.NonPlayerCharacter:
                case CreatureType.Monster:
                    if (creatureCreationArguments.Metadata?.Id == null)
                    {
                        throw new ArgumentException("Invalid metadata in creation arguments for a monster.", nameof(creatureCreationArguments));
                    }

                    using (var unitOfWork = this.applicationContext.CreateNewUnitOfWork())
                    {
                        var monsterType = unitOfWork.MonsterTypes.GetByRaceId(creatureCreationArguments.Metadata.Id);

                        if (monsterType == null)
                        {
                            throw new ArgumentException($"Unknown monster with Id {creatureCreationArguments.Metadata.Id} in creation arguments for a monster.", nameof(creatureCreationArguments));
                        }

                        // TODO: go through inventory composition here.
                        // For each inventory item (and chance or whatever), add the items to the monster (as IContainerThing),
                        // which will make the items fall in place, and also not have us have to pass the item factory into the monster instance, because it's weird.
                        return new Monster(monsterType);
                    }

                case CreatureType.Player:
                    if (creatureCreationArguments == null ||
                        creatureCreationArguments.Metadata == null ||
                        !(creatureCreationArguments is PlayerCreationArguments playerCreationArguments))
                    {
                        throw new ArgumentException("Invalid creation arguments for a player.", nameof(creatureCreationArguments));
                    }

                    return new Player(playerCreationArguments.CharacterMetadata, playerCreationArguments.PreselectedId);
            }

            throw new NotSupportedException($"{nameof(CreatureFactory)} does not support creation of creatures with type {creatureCreationArguments.Type}.");
        }
    }
}
