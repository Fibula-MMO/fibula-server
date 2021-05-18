// -----------------------------------------------------------------
// <copyright file="ItemsContainerExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Extensions
{
    using System.Collections.Generic;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Static class that provides helper methods for containers of items mechanics.
    /// </summary>
    public static class ItemsContainerExtensions
    {
        /// <summary>
        /// Attempts to add content to the first possible parent container that accepts it, on a chain of parent containers.
        /// </summary>
        /// <param name="thingContainer">The first thing container to add to.</param>
        /// <param name="itemFactory">The factory of items to use in case of splitting.</param>
        /// <param name="remainderItem">The remainder content to add, which overflows to the next container in the chain.</param>
        /// <param name="addIndex">The index at which to attempt to add, only for the first attempted container.</param>
        /// <param name="includeTileAsFallback">Optional. A value for whether to include tiles in the fallback chain.</param>
        /// <returns>True if the content was successfully added, false otherwise.</returns>
        public static bool AddItemToContainerRecursively(
            this IItemsContainer thingContainer,
            IItemFactory itemFactory,
            ref IItem remainderItem,
            byte addIndex = byte.MaxValue,
            bool includeTileAsFallback = true)
        {
            thingContainer.ThrowIfNull(nameof(thingContainer));
            itemFactory.ThrowIfNull(nameof(itemFactory));

            const byte FallbackIndex = byte.MaxValue;

            bool success = false;
            bool firstAttempt = true;

            foreach (var targetContainer in thingContainer.GetParentContainerHierarchy(includeTileAsFallback))
            {
                IThing lastAddedThing = remainderItem;

                if (!success)
                {
                    (success, remainderItem) = targetContainer.AddItem(itemFactory, remainderItem, firstAttempt ? addIndex : FallbackIndex);
                }
                else if (remainderItem != null)
                {
                    (success, remainderItem) = targetContainer.AddItem(itemFactory, remainderItem);
                }

                firstAttempt = false;

                if (success && remainderItem == null)
                {
                    break;
                }
            }

            return success;
        }

        /// <summary>
        /// Gets this thing's parent container hierarchy.
        /// </summary>
        /// <param name="itemsContainer">The container of items to get the hierarchy for.</param>
        /// <param name="includeTiles">Optional. A value indicating whether to include tiles in the hierarchy. Defaults to true.</param>
        /// <returns>The ordered collection of <see cref="IItemsContainer"/>s in this thing's container hierarchy.</returns>
        public static IEnumerable<IItemsContainer> GetParentContainerHierarchy(this IItemsContainer itemsContainer, bool includeTiles = true)
        {
            itemsContainer.ThrowIfNull(nameof(itemsContainer));

            IItemsContainer currentContainer = itemsContainer;

            while (currentContainer != null)
            {
                yield return currentContainer;

                if (currentContainer is IItem item)
                {
                    currentContainer = includeTiles || !(item.ParentContainer is ITile) ? item.ParentContainer : null;
                }

                if (currentContainer is ICreature creature)
                {
                    currentContainer = !includeTiles ? null : creature.ParentContainer as ITile;
                }
            }
        }
    }
}
