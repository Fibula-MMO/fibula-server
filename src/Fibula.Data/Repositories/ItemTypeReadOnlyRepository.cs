// -----------------------------------------------------------------
// <copyright file="ItemTypeReadOnlyRepository.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fibula.Data.Contracts.Abstractions;
    using Fibula.Data.Contracts.Abstractions.Repositories;
    using Fibula.Definitions.Data.Entities;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a read-only repository for item types.
    /// </summary>
    public class ItemTypeReadOnlyRepository : IReadOnlyRepository<ItemTypeEntity>
    {
        /// <summary>
        /// A locking object to prevent double initialization of the catalog.
        /// </summary>
        private static readonly object ItemTypeCatalogLock = new object();

        /// <summary>
        /// Stores the map between the item type ids and the actual item types.
        /// </summary>
        private static IDictionary<string, ItemTypeEntity> itemTypeCatalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTypeReadOnlyRepository"/> class.
        /// </summary>
        /// <param name="itemTypeLoader">A reference to the item type loader in use.</param>
        public ItemTypeReadOnlyRepository(IItemTypesLoader itemTypeLoader)
        {
            itemTypeLoader.ThrowIfNull(nameof(itemTypeLoader));

            if (itemTypeCatalog == null)
            {
                lock (ItemTypeCatalogLock)
                {
                    if (itemTypeCatalog == null)
                    {
                        itemTypeCatalog = itemTypeLoader.LoadTypes();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a monster type from the context.
        /// </summary>
        /// <param name="typeId">The id of the monster type to get.</param>
        /// <returns>The monster type found, if any.</returns>
        public static ItemTypeEntity GetByTypeId(string typeId)
        {
            typeId.ThrowIfNullOrWhiteSpace(nameof(typeId));

            if (itemTypeCatalog.ContainsKey(typeId))
            {
                return itemTypeCatalog[typeId];
            }

            return null;
        }

        /// <summary>
        /// Finds all the entities in the set within the context that satisfy an expression.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <returns>The collection of entities retrieved.</returns>
        public IEnumerable<ItemTypeEntity> FindMany(Expression<Func<ItemTypeEntity, bool>> predicate)
        {
            return itemTypeCatalog.Values.AsQueryable().Where(predicate);
        }

        /// <summary>
        /// Finds an entity in the set within the context that satisfies an expression.
        /// If more than one entity satisfies the expression, one is picked up in an unknown criteria.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <param name="includeProperties">Optional. Any additional properties to include.</param>
        /// <returns>The entity found.</returns>
        public ItemTypeEntity FindOne(Expression<Func<ItemTypeEntity, bool>> predicate, params string[] includeProperties)
        {
            return itemTypeCatalog.Values.AsQueryable().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets all the entities from the set in the context.
        /// </summary>
        /// <returns>The collection of entities retrieved.</returns>
        public Task<IEnumerable<ItemTypeEntity>> GetAll()
        {
            return Task.FromResult(itemTypeCatalog.Values.AsEnumerable());
        }

        /// <summary>
        /// Gets an entity by the primary key, from the context.
        /// </summary>
        /// <param name="keyMembersFunc">A function that returns the keys used to find the entity, in order.</param>
        /// <returns>The entity found, if any.</returns>
        public ItemTypeEntity GetByPrimaryKey(Func<object[]> keyMembersFunc)
        {
            keyMembersFunc.ThrowIfNull(nameof(keyMembersFunc));

            var key = keyMembersFunc();
            var typeId = key.FirstOrDefault()?.ToString();

            return GetByTypeId(typeId);
        }
    }
}
