// -----------------------------------------------------------------
// <copyright file="Creature.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Creatures
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Constants;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents all creatures in the game.
    /// </summary>
    public abstract class Creature : Thing, ICreature
    {
        /// <summary>
        /// Lock used when assigning creature ids.
        /// </summary>
        private static readonly object IdLock = new ();

        /// <summary>
        /// Counter to assign new ids to new creatures created.
        /// </summary>
        private static uint idCounter = 1;

        /// <summary>
        /// Stores the parent container for this creature.
        /// </summary>
        private ICreaturesContainer parentContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Creature"/> class.
        /// </summary>
        /// <param name="creationMetadata">The metadata for this player.</param>
        /// <param name="preselectedId">Optional. A pre-selected id to use for this creature. Defaults to 0 which picks a new id.</param>
        protected Creature(CreatureEntity creationMetadata, uint preselectedId = 0)
        {
            creationMetadata.ThrowIfNull(nameof(creationMetadata));
            creationMetadata.Name.ThrowIfNullOrWhiteSpace(nameof(creationMetadata.Name));

            if (creationMetadata.MaxHitpoints == 0)
            {
                throw new ArgumentException($"{nameof(creationMetadata.MaxHitpoints)} must be positive.", nameof(creationMetadata));
            }

            this.Id = preselectedId == 0 ? Creature.NewCreatureId() : preselectedId;

            this.Name = creationMetadata.Name;
            this.Article = creationMetadata.Article;
            this.CorpseTypeId = creationMetadata.Corpse;

            this.LastMovementCostModifier = 1;

            this.Outfit = new Outfit
            {
                Id = 0,
                ItemIdLookAlike = 0,
            };
        }

        /// <summary>
        /// Gets this thing's location.
        /// </summary>
        /// <remarks>
        /// A creatures's location is derived from the parent container, which should never be null (in the steady state)
        /// because a creature can only be standing on a tile, which has a location.
        /// </remarks>
        public override Location Location
        {
            get
            {
                return this.ParentContainer?.Location ?? default;
            }
        }

        /// <summary>
        /// Gets the location where this thing is being carried at, which is null for creatures.
        /// </summary>
        public override Location? CarryLocation
        {
            get
            {
                // Creatures cannot be carried.
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the parent container of this creature.
        /// </summary>
        public ICreaturesContainer ParentContainer
        {
            get
            {
                return this.parentContainer;
            }

            set
            {
                var oldLocation = this.Location;

                this.parentContainer = value;

                // Note that this.Location accounts for the parent container's location
                // That's why we check if these are now considered different.
                if (oldLocation != this.Location)
                {
                    this.RaiseLocationChanged(oldLocation);
                }
            }
        }

        /// <summary>
        /// Gets the type id of this creature.
        /// </summary>
        public override ushort TypeId => CreatureConstants.CreatureTypeId;

        /// <summary>
        /// Gets the creature's in-game id.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Gets the article in the name of the creature.
        /// </summary>
        public string Article { get; }

        /// <summary>
        /// Gets the name of the creature.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the corpse of the creature.
        /// </summary>
        public ushort CorpseTypeId { get; }

        /// <summary>
        /// Gets or sets this creature's light level.
        /// </summary>
        public byte EmittedLightLevel { get; protected set; }

        /// <summary>
        /// Gets or sets this creature's light color.
        /// </summary>
        public byte EmittedLightColor { get; protected set; }

        /// <summary>
        /// Gets this creature's speed.
        /// </summary>
        public abstract ushort Speed { get; }

        /// <summary>
        /// Gets or sets this creature's variable speed.
        /// </summary>
        public int VariableSpeed { get; protected set; }

        /// <summary>
        /// Gets this creature's flags.
        /// </summary>
        public uint Flags { get; private set; }

        /// <summary>
        /// Gets or sets this creature's blood type.
        /// </summary>
        public BloodType BloodType { get; protected set; }

        /// <summary>
        /// Gets or sets the outfit of this creature.
        /// </summary>
        public Outfit Outfit { get; set; }

        /// <summary>
        /// Gets or sets the direction that this creature is facing.
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Gets or sets the creature's last move modifier.
        /// </summary>
        public decimal LastMovementCostModifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether the creature is considered dead.
        /// </summary>
        public bool IsDead => false;

        /// <summary>
        /// Gets a value indicating whether this creature can walk.
        /// </summary>
        public bool CanWalk => this.Speed > 0;

        /// <summary>
        /// Provides a string describing the current creature for logging purposes.
        /// </summary>
        /// <returns>The string to log.</returns>
        public override string DescribeForLogger()
        {
            return $"{(string.IsNullOrWhiteSpace(this.Article) ? string.Empty : $"{this.Article} ")}{this.Name}";
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">The other object to compare against.</param>
        /// <returns>True if the current object is equal to the other parameter, false otherwise.</returns>
        public bool Equals([AllowNull] ICreature other)
        {
            return this.Id == other?.Id;
        }

        /// <summary>
        /// Attempts to add an <see cref="IItem"/> to this creature.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be added.</param>
        /// <param name="itemToAdd">The item to add to the creature.</param>
        /// <param name="atIndex">Optional. The index at which to add the item. Defaults to a value of <see cref="byte.MaxValue"/>, which means to try adding at any free index.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the item may be returned.</returns>
        public (bool result, IItem remainder) AddItem(IItemFactory itemFactory, IItem itemToAdd, byte atIndex = byte.MaxValue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to remove an <see cref="IItem"/> from this creature.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be removed, like when a smaller <paramref name="amount"/> than what is available is specified.</param>
        /// <param name="itemToRemove">The item to remove from the creature.</param>
        /// <param name="amount">Optional. The amount of the <paramref name="itemToRemove"/> to remove.</param>
        /// <param name="index">Optional. The index from which to remove the item. Defaults to <see cref="byte.MaxValue"/>, which means to try removing the item at any index that it's found.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the item may be returned.</returns>
        public (bool result, IItem remainder) RemoveItem(IItemFactory itemFactory, ref IItem itemToRemove, byte amount = 1, byte index = byte.MaxValue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to replace an <see cref="IItem"/> from this creature by removing the <paramref name="itemToRemove"/> and adding the <paramref name="itemToAdd"/>.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be removed, like when a smaller <paramref name="amount"/> than what is available is specified.</param>
        /// <param name="itemToRemove">The item to remove from the creature.</param>
        /// <param name="itemToAdd">The item to add to the creature.</param>
        /// <param name="amount">Optional. The amount of the <paramref name="itemToRemove"/> to replace.</param>
        /// <param name="index">Optional. The index from which to remove the item. Defaults to <see cref="byte.MaxValue"/>, which means to try removing the item at any index that it's found.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the thing may be returned.</returns>
        public (bool result, IItem remainderToChange) ReplaceItem(IItemFactory itemFactory, IItem itemToRemove, IItem itemToAdd, byte amount = 1, byte index = byte.MaxValue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to find an <see cref="IItem"/> in this creature, at a given index.
        /// </summary>
        /// <param name="index">The index at which to look.</param>
        /// <returns>The <see cref="IItem"/> found at the index, if any was found, and null otherwise.</returns>
        public IItem FindItemAtIndex(byte index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to find an <see cref="IItem"/> in this creature, of a given type.
        /// </summary>
        /// <param name="typeId">The id of the type to look for.</param>
        /// <returns>The item found, if any, and null otherwise.</returns>
        public IItem FindItemByTypeId(ushort typeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Selects and returns a new in-game id for a creature.
        /// </summary>
        /// <returns>The picked id.</returns>
        protected static uint NewCreatureId()
        {
            lock (Creature.IdLock)
            {
                return idCounter++;
            }
        }
    }
}
