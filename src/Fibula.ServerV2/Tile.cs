// -----------------------------------------------------------------
// <copyright file="Tile.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Constants;
    using Fibula.ServerV2.Contracts.Enumerations;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a tile in the map.
    /// </summary>
    public class Tile : ITile
    {
        /// <summary>
        /// The object to use as the tile lock.
        /// </summary>
        private readonly object tileLock;

        /// <summary>
        /// Stores the creatures on the tile.
        /// </summary>
        private readonly Stack<ICreature> creaturesOnTile;

        /// <summary>
        /// Stores the 'top' items on the tile.
        /// </summary>
        private readonly Stack<IItem> stayOnTopItems;

        /// <summary>
        /// Stores the 'top 2' items on the tile.
        /// </summary>
        private readonly Stack<IItem> stayOnBottomItems;

        /// <summary>
        /// Stores the items on the tile.
        /// </summary>
        private readonly Stack<IItem> itemsOnTile;

        /// <summary>
        /// Stores the items on the tile that are ground borders.
        /// </summary>
        private readonly Stack<IItem> groundBorders;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="location">The location of this tile.</param>
        /// <param name="ground">The ground item to initialize the tile with.</param>
        public Tile(Location location, IItem ground)
        {
            if (location.Type != LocationType.Map)
            {
                throw new ArgumentException($"Invalid location {location} for tile. A tile must have a {LocationType.Map} location.");
            }

            this.Location = location;

            this.Ground = ground;
            this.Flags = (byte)TileFlag.None;
            this.LastModified = DateTimeOffset.UtcNow;

            this.tileLock = new object();
            this.groundBorders = new Stack<IItem>();
            this.creaturesOnTile = new Stack<ICreature>();
            this.stayOnTopItems = new Stack<IItem>();
            this.stayOnBottomItems = new Stack<IItem>();
            this.itemsOnTile = new Stack<IItem>();
        }

        /// <summary>
        /// Gets this tile's location.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Gets the location where this entity is being carried at, which is null for tiles.
        /// </summary>
        public Location? CarryLocation => null;

        /// <summary>
        /// Gets the single ground item that a tile may have.
        /// </summary>
        public IItem Ground { get; private set; }

        /// <summary>
        /// Gets the single liquid pool item that a tile may have.
        /// </summary>
        public IItem LiquidPool { get; private set; }

        /// <summary>
        /// Gets the tile's creature ids.
        /// </summary>
        public IEnumerable<ICreature> Creatures => this.creaturesOnTile;

        /// <summary>
        /// Gets the flags from this tile.
        /// </summary>
        public byte Flags { get; private set; }

        /// <summary>
        /// Gets the last date and time that this tile was modified.
        /// </summary>
        public DateTimeOffset LastModified { get; private set; }

        /// <summary>
        /// Gets a value indicating whether items in this tile block throwing.
        /// </summary>
        public bool BlocksThrow
        {
            get
            {
                // TODO: handle setting this as the items get added/removed to avoid constant calculation.
                return (this.Ground != null && this.Ground.BlocksThrow) ||
                        this.groundBorders.Any(i => i.BlocksThrow) ||
                        (this.LiquidPool != null && this.LiquidPool.BlocksThrow) ||
                        this.stayOnTopItems.Any(i => i.BlocksThrow) ||
                        this.stayOnBottomItems.Any(i => i.BlocksThrow) ||
                        this.itemsOnTile.Any(i => i.BlocksThrow);
            }
        }

        /// <summary>
        /// Gets a value indicating whether items in this tile block passing.
        /// </summary>
        public bool BlocksPass
        {
            get
            {
                // TODO: handle setting this as the items get added/removed to avoid constant calculation.
                return (this.Ground != null && this.Ground.BlocksPass) ||
                        this.Creatures.Any() ||
                        this.groundBorders.Any(i => i.BlocksPass) ||
                        (this.LiquidPool != null && this.LiquidPool.BlocksPass) ||
                        this.stayOnTopItems.Any(i => i.BlocksPass) ||
                        this.stayOnBottomItems.Any(i => i.BlocksPass) ||
                        this.itemsOnTile.Any(i => i.BlocksPass);
            }
        }

        /// <summary>
        /// Gets a value indicating whether items in this tile block laying.
        /// </summary>
        public bool BlocksLay
        {
            get
            {
                // TODO: handle setting this as the items get added/removed to avoid constant calculation.
                return (this.Ground != null && this.Ground.BlocksLay) ||
                        this.groundBorders.Any(i => i.BlocksLay) ||
                        (this.LiquidPool != null && this.LiquidPool.BlocksLay) ||
                        this.stayOnTopItems.Any(i => i.BlocksLay) ||
                        this.stayOnBottomItems.Any(i => i.BlocksLay) ||
                        this.itemsOnTile.Any(i => i.BlocksLay);
            }
        }

        /// <summary>
        /// Gets the thing that is on top based on the tile's stack order. Prioritizes creatures, then items.
        /// </summary>
        public IThing TopThing
        {
            get
            {
                return (IThing)this.TopCreature ?? this.TopItem;
            }
        }

        /// <summary>
        /// Gets the creature that is on top based on the tile's stack order.
        /// </summary>
        public ICreature TopCreature
        {
            get
            {
                return this.creaturesOnTile.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the item that is on top based on the tile's stack order.
        /// </summary>
        public IItem TopItem
        {
            get
            {
                return this.itemsOnTile.FirstOrDefault() ??
                       this.stayOnBottomItems.FirstOrDefault() ??
                       this.stayOnTopItems.FirstOrDefault() ??
                       this.LiquidPool ??
                       this.groundBorders.FirstOrDefault() ??
                       this.Ground;
            }
        }

        /// <summary>
        /// Attempts to get the tile's items to describe prioritized and ordered by their stack order.
        /// </summary>
        /// <param name="maxItemsToGet">The maximum number of items to include in the result.</param>
        /// <returns>The items in the tile, split by those which are fixed and those considered normal.</returns>
        /// <remarks>
        /// The algorithm prioritizes the returned items in the following order:
        /// 1) Ground item.
        /// 2) Ground border items.
        /// 3) Liquid pool item.
        /// 4) Stay-on-bottom items.
        /// 5) Stay-on-top items.
        /// 6) Normal items.
        /// </remarks>
        public (IEnumerable<IItem> fixedItems, IEnumerable<IItem> normalItems) GetItemsToDescribeByPriority(int maxItemsToGet = MapConstants.MaximumNumberOfThingsToDescribePerTile)
        {
            var fixedItemList = new List<IItem>();
            var itemList = new List<IItem>();
            var addedCount = 0;
            var ultimited = maxItemsToGet < 0;

            lock (this.tileLock)
            {
                if (this.Ground != null)
                {
                    fixedItemList.Add(this.Ground);
                    addedCount++;
                }

                if ((ultimited || addedCount < maxItemsToGet) && this.groundBorders.Any())
                {
                    var itemsToAdd = this.groundBorders.Take(maxItemsToGet - addedCount);

                    fixedItemList.AddRange(itemsToAdd);

                    addedCount += itemsToAdd.Count();
                }

                if ((ultimited || addedCount < maxItemsToGet) && this.LiquidPool != null)
                {
                    fixedItemList.Add(this.LiquidPool);
                    addedCount++;
                }

                if ((ultimited || addedCount < maxItemsToGet) && this.stayOnTopItems.Any())
                {
                    var itemsToAdd = this.stayOnTopItems.Take(maxItemsToGet - addedCount);

                    fixedItemList.AddRange(itemsToAdd);

                    addedCount += itemsToAdd.Count();
                }

                if ((ultimited || addedCount < maxItemsToGet) && this.stayOnBottomItems.Any())
                {
                    var itemsToAdd = this.stayOnBottomItems.Take(maxItemsToGet - addedCount);

                    fixedItemList.AddRange(itemsToAdd);

                    addedCount += itemsToAdd.Count();
                }

                if ((ultimited || addedCount < maxItemsToGet) && this.itemsOnTile.Any())
                {
                    var itemsToAdd = this.itemsOnTile.Take(maxItemsToGet - addedCount);

                    itemList.AddRange(itemsToAdd);

                    addedCount += itemsToAdd.Count();
                }
            }

            return (fixedItemList, itemList);
        }

        /// <summary>
        /// Sets a flag on this tile.
        /// </summary>
        /// <param name="flag">The flag to set.</param>
        public void SetFlag(TileFlag flag)
        {
            this.Flags |= (byte)flag;
        }

        /// <summary>
        /// Attempts to find an item in the tile with the given type.
        /// </summary>
        /// <param name="typeId">The type to look for.</param>
        /// <returns>The item with such id, null otherwise.</returns>
        public IItem FindItemWithTypeId(ushort typeId)
        {
            if (this.Ground != null && this.Ground.TypeId == typeId)
            {
                return this.Ground;
            }

            if (this.LiquidPool != null && this.LiquidPool.TypeId == typeId)
            {
                return this.LiquidPool;
            }

            lock (this.tileLock)
            {
                return this.groundBorders.FirstOrDefault(i => i.TypeId == typeId) ?? this.stayOnTopItems.FirstOrDefault(i => i.TypeId == typeId) ?? this.stayOnBottomItems.FirstOrDefault(i => i.TypeId == typeId) ?? this.itemsOnTile.FirstOrDefault(i => i.TypeId == typeId);
            }
        }

        /// <summary>
        /// Attempts to get the index for the given <see cref="IThing"/> within this container.
        /// </summary>
        /// <param name="thing">The thing to find.</param>
        /// <returns>The index for the <see cref="IThing"/> found, or <see cref="byte.MaxValue"/> if it was not found.</returns>
        public byte GetIndexOfThing(IThing thing)
        {
            thing.ThrowIfNull(nameof(thing));

            byte i = (byte)(this.Ground != null ? 1 : 0);

            if (this.Ground != null && thing == this.Ground)
            {
                return i;
            }

            foreach (var item in this.groundBorders)
            {
                if (thing == item)
                {
                    return i;
                }

                i++;
            }

            if (this.LiquidPool != null)
            {
                if (thing == this.LiquidPool)
                {
                    return i;
                }

                i++;
            }

            foreach (var item in this.stayOnTopItems)
            {
                if (thing == item)
                {
                    return i;
                }

                i++;
            }

            foreach (var item in this.stayOnBottomItems)
            {
                if (thing == item)
                {
                    return i;
                }

                i++;
            }

            foreach (var creatureOnTile in this.creaturesOnTile)
            {
                if (thing is ICreature creature && creature == creatureOnTile)
                {
                    return i;
                }

                i++;
            }

            foreach (var item in this.itemsOnTile)
            {
                if (thing == item)
                {
                    return i;
                }

                i++;
            }

            return byte.MaxValue;
        }

        /// <summary>
        /// Attempts to find a <see cref="IThing"/> in the container, at the given index.
        /// </summary>
        /// <param name="index">The index at which to look for.</param>
        /// <returns>The <see cref="IThing"/> found at the index, and null otherwise.</returns>
        public IThing FindThingAtIndex(byte index)
        {
            var currentIdx = 0;

            lock (this.tileLock)
            {
                if (this.Ground != null && currentIdx++ == index)
                {
                    return this.Ground;
                }

                if (this.groundBorders.Any())
                {
                    if (currentIdx + this.groundBorders.Count > index)
                    {
                        return this.groundBorders.Skip(index - currentIdx).Single();
                    }

                    currentIdx += this.groundBorders.Count;
                }

                if (this.LiquidPool != null && currentIdx++ == index)
                {
                    return this.LiquidPool;
                }

                if (this.stayOnTopItems.Any())
                {
                    if (currentIdx + this.stayOnTopItems.Count > index)
                    {
                        return this.stayOnTopItems.Skip(index - currentIdx).Single();
                    }

                    currentIdx += this.stayOnTopItems.Count;
                }

                if (this.stayOnBottomItems.Any())
                {
                    if (currentIdx + this.stayOnBottomItems.Count > index)
                    {
                        return this.stayOnBottomItems.Skip(index - currentIdx).Single();
                    }

                    currentIdx += this.stayOnBottomItems.Count;
                }

                if (this.itemsOnTile.Any())
                {
                    if (currentIdx + this.itemsOnTile.Count > index)
                    {
                        return this.itemsOnTile.Skip(index - currentIdx).Single();
                    }

                    currentIdx += this.itemsOnTile.Count;
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to add an <see cref="IItem"/> to this container.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be added.</param>
        /// <param name="itemToAdd">The item to add to the container.</param>
        /// <param name="atIndex">Optional. The index at which to add the item. Defaults to a value of <see cref="byte.MaxValue"/>, which means to try adding at any free index.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the item may be returned.</returns>
        public (bool result, IItem remainder) AddItem(IItemFactory itemFactory, IItem itemToAdd, byte atIndex = byte.MaxValue)
        {
            itemFactory.ThrowIfNull(nameof(itemFactory));

            lock (this.tileLock)
            {
                if (itemToAdd.IsGround)
                {
                    this.Ground = itemToAdd;
                }
                else if (itemToAdd.IsGroundFix)
                {
                    this.groundBorders.Push(itemToAdd);
                }
                else if (itemToAdd.IsLiquidPool)
                {
                    this.LiquidPool = itemToAdd;
                }
                else if (itemToAdd.StaysOnTop)
                {
                    this.stayOnTopItems.Push(itemToAdd);
                }
                else if (itemToAdd.StaysOnBottom)
                {
                    this.stayOnBottomItems.Push(itemToAdd);
                }
                else
                {
                    var remainingAmountToAdd = itemToAdd.Amount;

                    while (remainingAmountToAdd > 0)
                    {
                        if (!itemToAdd.IsCumulative)
                        {
                            this.itemsOnTile.Push(itemToAdd);
                            break;
                        }

                        var existingItem = this.itemsOnTile.Count > 0 ? this.itemsOnTile.Peek() : null;

                        // Check if there is an existing top itemToAdd and if it is of the same type.
                        if (existingItem == null || existingItem.Type != itemToAdd.Type || existingItem.Amount >= ItemConstants.MaximumAmountOfCummulativeItems)
                        {
                            this.itemsOnTile.Push(itemToAdd);
                            break;
                        }

                        remainingAmountToAdd += existingItem.Amount;

                        // Modify the existing itemToAdd with the new amount, or the maximum permitted.
                        var newExistingAmount = Math.Min(remainingAmountToAdd, ItemConstants.MaximumAmountOfCummulativeItems);

                        existingItem.Amount = newExistingAmount;

                        remainingAmountToAdd -= newExistingAmount;

                        if (remainingAmountToAdd == 0)
                        {
                            break;
                        }

                        itemToAdd = itemToAdd.Clone();

                        itemToAdd.Amount = remainingAmountToAdd;

                        itemToAdd.ParentContainer = this;
                    }
                }

                // Update the tile's version so that it invalidates the cache.
                this.LastModified = DateTimeOffset.UtcNow;
            }

            return (true, null);
        }

        /// <summary>
        /// Attempts to remove an <see cref="IItem"/> from this container.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be removed, like when a smaller <paramref name="amount"/> than what is available is specified.</param>
        /// <param name="itemToRemove">The item to remove from the container.</param>
        /// <param name="amount">Optional. The amount of the <paramref name="itemToRemove"/> to remove.</param>
        /// <param name="index">Optional. The index from which to remove the item. Defaults to <see cref="byte.MaxValue"/>, which means to try removing the item at any index that it's found.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the item may be returned.</returns>
        public (bool result, IItem remainder) RemoveItem(IItemFactory itemFactory, ref IItem itemToRemove, byte amount = 1, byte index = byte.MaxValue)
        {
            if (amount == 0)
            {
                throw new ArgumentException($"Invalid {nameof(amount)} zero.");
            }

            IItem remainder = null;

            if (itemToRemove.IsGround)
            {
                if (this.Ground != itemToRemove)
                {
                    return (false, itemToRemove);
                }

                this.Ground = null;
            }
            else if (itemToRemove.IsGroundFix)
            {
                if (amount > 1)
                {
                    throw new ArgumentException($"Invalid {nameof(amount)} while removing a ground border itemToRemove: {amount}.");
                }

                return (this.InternalRemoveGroundBorderItem(itemToRemove), null);
            }
            else if (itemToRemove.IsLiquidPool)
            {
                if (this.LiquidPool != itemToRemove)
                {
                    return (false, itemToRemove);
                }

                this.LiquidPool = null;
            }
            else if (itemToRemove.StaysOnTop)
            {
                if (amount > 1)
                {
                    throw new ArgumentException($"Invalid {nameof(amount)} while removing a stay-on-top itemToRemove: {amount}.");
                }

                return (this.InternalRemoveStayOnTopItem(itemToRemove), null);
            }
            else if (itemToRemove.StaysOnBottom)
            {
                if (amount > 1)
                {
                    throw new ArgumentException($"Invalid {nameof(amount)} while removing a stay-on-bottom itemToRemove: {amount}.");
                }

                return (this.InternalRemoveStayOnBottomItem(itemToRemove), null);
            }
            else
            {
                lock (this.tileLock)
                {
                    if ((!itemToRemove.IsCumulative && amount > 1) || (itemToRemove.IsCumulative && itemToRemove.Amount < amount))
                    {
                        return (false, null);
                    }

                    if (!itemToRemove.IsCumulative || itemToRemove.Amount == amount)
                    {
                        // Since we have the exact amount, we can remove the itemToRemove instance from the tile.
                        this.itemsOnTile.Pop();
                    }
                    else
                    {
                        // We're removing less than the entire amount, so we need to calculate the remainder to add back.
                        var newExistingAmount = (byte)(itemToRemove.Amount - amount);

                        itemToRemove.Amount = newExistingAmount;

                        // Create a new itemToRemove as the remainder.
                        remainder = itemToRemove.Clone();

                        remainder.Amount = amount;

                        itemToRemove = remainder;
                        remainder = itemToRemove;
                    }
                }
            }

            // Update the tile's version so that it invalidates the cache.
            this.LastModified = DateTimeOffset.UtcNow;

            return (true, remainder);
        }

        /// <summary>
        /// Attempts to replace an <see cref="IItem"/> from this container by removing the <paramref name="itemToRemove"/> and adding the <paramref name="itemToAdd"/>.
        /// </summary>
        /// <param name="itemFactory">A reference to a factory of items. Used in case the item can only partially be removed, like when a smaller <paramref name="amount"/> than what is available is specified.</param>
        /// <param name="itemToRemove">The item to remove from the container.</param>
        /// <param name="itemToAdd">The item to add to the container.</param>
        /// <param name="amount">Optional. The amount of the <paramref name="itemToRemove"/> to replace.</param>
        /// <param name="index">Optional. The index from which to remove the item. Defaults to <see cref="byte.MaxValue"/>, which means to try removing the item at any index that it's found.</param>
        /// <returns>A tuple with a value indicating whether the attempt was at least partially successful, and false otherwise. If the result was only partially successful, a remainder of the thing may be returned.</returns>
        public (bool result, IItem remainderToChange) ReplaceItem(IItemFactory itemFactory, IItem itemToRemove, IItem itemToAdd, byte amount = 1, byte index = byte.MaxValue)
        {
            (bool removeSuccessful, IItem removeRemainder) = this.RemoveItem(itemFactory, ref itemToRemove, index, amount);

            if (!removeSuccessful)
            {
                return (false, removeRemainder);
            }

            if (removeRemainder != null)
            {
                (bool addedRemainder, IItem remainderOfRemainder) = this.AddItem(itemFactory, removeRemainder, byte.MaxValue);

                if (!addedRemainder)
                {
                    return (false, remainderOfRemainder);
                }
            }

            return this.AddItem(itemFactory, itemToAdd, index);
        }

        /// <summary>
        /// Attempts to find an <see cref="IItem"/> in this container, at a given index.
        /// </summary>
        /// <param name="index">The index at which to look.</param>
        /// <returns>The <see cref="IItem"/> found at the index, if any was found, and null otherwise.</returns>
        public IItem FindItemAtIndex(byte index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to find an <see cref="IItem"/> in this container, of a given type.
        /// </summary>
        /// <param name="typeId">The id of the type to look for.</param>
        /// <returns>The item found, if any, and null otherwise.</returns>
        public IItem FindItemByTypeId(ushort typeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to add an <see cref="ICreature"/> to this container.
        /// </summary>
        /// <param name="creature">The creature to add to the container.</param>
        /// <returns>True if the creature was successfully added, and false otherwise.</returns>
        public bool AddCreature(ICreature creature)
        {
            this.creaturesOnTile.Push(creature);

            creature.ParentContainer = this;

            return true;
        }

        /// <summary>
        /// Attempts to remove an <see cref="ICreature"/> from this container.
        /// </summary>
        /// <param name="creature">The creature to remove from the container.</param>
        /// <returns>True if the creature was successfully removed, and false otherwise.</returns>
        public bool RemoveCreature(ICreature creature)
        {
            return this.InternalRemoveCreature(creature);
        }

        /// <summary>
        /// Attempts to remove the given creature id from the stack of this tile.
        /// </summary>
        /// <param name="creature">The creature to attempt to remove.</param>
        /// <returns>True if the id is found and removed, false otherwise.</returns>
        private bool InternalRemoveCreature(ICreature creature)
        {
            var tempStack = new Stack<ICreature>();

            ICreature removedCreature = null;

            lock (this.tileLock)
            {
                while (removedCreature == default && this.creaturesOnTile.Count > 0)
                {
                    var tempCreature = this.creaturesOnTile.Pop();

                    if (creature == tempCreature)
                    {
                        removedCreature = creature;
                    }
                    else
                    {
                        tempStack.Push(tempCreature);
                    }
                }

                while (tempStack.Count > 0)
                {
                    this.creaturesOnTile.Push(tempStack.Pop());
                }
            }

            return removedCreature != null;
        }

        /// <summary>
        /// Attempts to remove a specific item of the ground borders category in this tile.
        /// </summary>
        /// <param name="groundBorderItem">The item to remove.</param>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        private bool InternalRemoveGroundBorderItem(IItem groundBorderItem)
        {
            if (groundBorderItem == null)
            {
                return false;
            }

            var tempStack = new Stack<IItem>();

            bool wasRemoved = false;

            lock (this.tileLock)
            {
                while (!wasRemoved && this.groundBorders.Count > 0)
                {
                    var temp = this.groundBorders.Pop();

                    if (groundBorderItem == temp)
                    {
                        wasRemoved = true;
                        break;
                    }
                    else
                    {
                        tempStack.Push(temp);
                    }
                }

                while (tempStack.Count > 0)
                {
                    this.groundBorders.Push(tempStack.Pop());
                }
            }

            if (wasRemoved)
            {
                // Update the tile's version so that it invalidates the cache.
                this.LastModified = DateTimeOffset.UtcNow;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Attempts to remove a specific item of the stay-on-top category in this tile.
        /// </summary>
        /// <param name="stayOnTopItem">The item to remove.</param>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        private bool InternalRemoveStayOnTopItem(IItem stayOnTopItem)
        {
            if (stayOnTopItem == null)
            {
                return false;
            }

            var tempStack = new Stack<IItem>();

            bool wasRemoved = false;

            lock (this.tileLock)
            {
                while (!wasRemoved && this.stayOnTopItems.Count > 0)
                {
                    var temp = this.stayOnTopItems.Pop();

                    if (stayOnTopItem == temp)
                    {
                        wasRemoved = true;
                        break;
                    }
                    else
                    {
                        tempStack.Push(temp);
                    }
                }

                while (tempStack.Count > 0)
                {
                    this.stayOnTopItems.Push(tempStack.Pop());
                }
            }

            if (wasRemoved)
            {
                // Update the tile's version so that it invalidates the cache.
                this.LastModified = DateTimeOffset.UtcNow;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Attempts to remove a specific item of the stay-on-bottom category in this tile.
        /// </summary>
        /// <param name="stayOnBottomItem">The item to remove.</param>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        private bool InternalRemoveStayOnBottomItem(IItem stayOnBottomItem)
        {
            if (stayOnBottomItem == null)
            {
                return false;
            }

            var tempStack = new Stack<IItem>();

            bool wasRemoved = false;

            lock (this.tileLock)
            {
                while (!wasRemoved && this.stayOnBottomItems.Count > 0)
                {
                    var temp = this.stayOnBottomItems.Pop();

                    if (stayOnBottomItem == temp)
                    {
                        wasRemoved = true;
                        break;
                    }
                    else
                    {
                        tempStack.Push(temp);
                    }
                }

                while (tempStack.Count > 0)
                {
                    this.stayOnBottomItems.Push(tempStack.Pop());
                }
            }

            if (wasRemoved)
            {
                // Update the tile's version so that it invalidates the cache.
                this.LastModified = DateTimeOffset.UtcNow;
            }

            return wasRemoved;
        }
    }
}
