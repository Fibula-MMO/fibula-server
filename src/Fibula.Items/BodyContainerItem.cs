﻿// -----------------------------------------------------------------
// <copyright file="BodyContainerItem.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Items
{
    using System;
    using System.Linq;
    using Fibula.Common.Contracts.Abstractions;
    using Fibula.Common.Contracts.Constants;
    using Fibula.Common.Contracts.Enumerations;
    using Fibula.Common.Contracts.Structs;
    using Fibula.Creatures.Contracts.Abstractions;
    using Fibula.Data.Entities;
    using Fibula.Items.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a container item for body slots.
    /// </summary>
    public class BodyContainerItem : ContainerItem
    {
        /// <summary>
        /// Stores this body container's location.
        /// </summary>
        private Location location;

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyContainerItem"/> class.
        /// </summary>
        /// <param name="slot">The slot at which this body container is.</param>
        public BodyContainerItem(Slot slot)
            : base(new ItemTypeEntity())
        {
            this.Slot = slot;

            this.location = new Location()
            {
                X = LocationConstants.NonMapLocationX,
                Y = (byte)this.Slot,
            };
        }

        /// <summary>
        /// Gets the capacity of this container.
        /// Body container items all have a fixed capacity of 1.
        /// </summary>
        public override byte Capacity => 0x01;

        /// <summary>
        /// Gets this body container's slot.
        /// </summary>
        public Slot Slot { get; }

        /// <summary>
        /// Gets this body container's location, which should fall back to the creature that owns it.
        /// </summary>
        public override Location Location
        {
            get
            {
                return this.ParentContainer?.Location ?? this.location;
            }
        }

        /// <summary>
        /// Gets the location where this thing is being carried at.
        /// </summary>
        public override Location? CarryLocation
        {
            get
            {
                return this.location;
            }
        }

        /// <summary>
        /// Attempts to add a <see cref="IThing"/> to this container.
        /// </summary>
        /// <param name="thingFactory">A reference to the factory of things to use.</param>
        /// <param name="thing">The <see cref="IThing"/> to add to the container.</param>
        /// <param name="index">Optional. The index at which to add the <see cref="IThing"/>. Defaults to byte.MaxValue, which instructs to add the <see cref="IThing"/> at any free index.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the thing may be returned.</returns>
        public override (bool result, IThing remainder) AddContent(IThingFactory thingFactory, IThing thing, byte index = byte.MaxValue)
        {
            thingFactory.ThrowIfNull(nameof(thingFactory));
            thing.ThrowIfNull(nameof(thing));

            if (thingFactory is not IItemFactory itemFactory)
            {
                throw new ArgumentException($"The {nameof(thingFactory)} must be derived of type {nameof(IItemFactory)}.");
            }

            if (thing is not IItem item)
            {
                // Containers like this can only add items.
                return (false, null);
            }

            // Validate that the item being added is not a parent of this item.
            if (this.IsChildOf(item) || !this.CanDressItemHere(item))
            {
                // TODO: error message 'This is impossible'.
                return (false, thing);
            }

            // Find an index which falls in within the actual content boundaries.
            var targetIndex = index < this.Content.Count ? index : -1;

            // Then get an item if there is one, at that index.
            var existingItemAtIndex = targetIndex == -1 ? this.Content.FirstOrDefault() : this.Content[targetIndex];

            (bool success, IThing remainderItem) = (false, item);

            if (existingItemAtIndex != null)
            {
                // We matched with an item, let's attempt to add or join with it first.
                if (existingItemAtIndex.IsContainer && existingItemAtIndex is IContainerItem existingContainer)
                {
                    return existingContainer.AddContent(itemFactory, remainderItem);
                }
                else
                {
                    (success, remainderItem) = existingItemAtIndex.Merge(remainderItem as IItem);

                    if (success)
                    {
                        // Regardless if we're done, we've changed an item at this index, so we notify observers.
                        this.InvokeContentUpdated((byte)targetIndex, remainderItem as IItem);
                    }
                }
            }

            if (success)
            {
                // If we have partially succeeded, we need to return now.
                return (true, remainderItem);
            }

            if (existingItemAtIndex == null)
            {
                remainderItem.ParentContainer = this;

                this.Content.Insert(0, remainderItem as IItem);

                this.InvokeContentAdded(remainderItem as IItem);

                remainderItem = null;
            }
            else
            {
                // Swap the items.
                this.Content.Clear();

                remainderItem.ParentContainer = this;

                this.Content.Insert(0, remainderItem as IItem);
                this.InvokeContentUpdated(0, remainderItem as IItem);

                remainderItem = existingItemAtIndex;
            }

            return (true, remainderItem);
        }

        /// <summary>
        /// Checks if the given thing can be dressed in this body container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the thing can be dressed in this container, false otherwise.</returns>
        private bool CanDressItemHere(IItem item)
        {
            if (item == null || this.ParentContainer is not IPlayer player)
            {
                return false;
            }

            switch (this.Slot)
            {
                // Not valid targets
                default:
                case Slot.UnsetInvalid:
                case Slot.Anywhere:
                    return false;

                // Valid target, wildcard slot
                case Slot.Ammo:
                    return true;

                // Valid target, straightforward slots
                case Slot.Head:
                case Slot.Neck:
                case Slot.Back:
                case Slot.Body:
                case Slot.Legs:
                case Slot.Ring:
                case Slot.Feet:
                    return item.CanBeDressed && item.DressPosition == this.Slot;

                // Valid target, special slots
                case Slot.LeftHand:
                    if (player.Inventory[(byte)Slot.RightHand] is not IContainerItem rightHandContainer)
                    {
                        return false;
                    }

                    var rightHandItem = rightHandContainer.Content.FirstOrDefault();

                    return rightHandItem == null || (item.DressPosition != Slot.TwoHanded && rightHandItem.DressPosition != Slot.TwoHanded);

                case Slot.RightHand:
                    if (player.Inventory[(byte)Slot.LeftHand] is not IContainerItem leftHandContainer)
                    {
                        return false;
                    }

                    var leftHandItem = leftHandContainer.Content.FirstOrDefault();

                    return leftHandItem == null || (item.DressPosition != Slot.TwoHanded && leftHandItem.DressPosition != Slot.TwoHanded);
            }
        }
    }
}
