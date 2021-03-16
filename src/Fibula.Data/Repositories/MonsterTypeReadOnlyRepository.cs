// -----------------------------------------------------------------
// <copyright file="MonsterTypeReadOnlyRepository.cs" company="2Dudes">
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
    /// Class that represents a read-only repository for monster types.
    /// </summary>
    public class MonsterTypeReadOnlyRepository : IReadOnlyRepository<MonsterTypeEntity>
    {
        /// <summary>
        /// A locking object to prevent double initialization of the catalog.
        /// </summary>
        private static readonly object MonsterTypeCatalogLock = new object();

        /// <summary>
        /// Stores the map between the monster race ids and the actual monster types.
        /// </summary>
        private static IDictionary<string, MonsterTypeEntity> monsterTypeCatalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonsterTypeReadOnlyRepository"/> class.
        /// </summary>
        /// <param name="monsterTypeLoader">A reference to the monster type loader in use.</param>
        public MonsterTypeReadOnlyRepository(IMonsterTypesLoader monsterTypeLoader)
        {
            monsterTypeLoader.ThrowIfNull(nameof(monsterTypeLoader));

            if (monsterTypeCatalog == null)
            {
                lock (MonsterTypeCatalogLock)
                {
                    if (monsterTypeCatalog == null)
                    {
                        monsterTypeCatalog = monsterTypeLoader.LoadTypes();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a monster type from the context.
        /// </summary>
        /// <param name="raceId">The id of the monster type to get.</param>
        /// <returns>The monster type found, if any.</returns>
        public static MonsterTypeEntity GetByRaceId(string raceId)
        {
            raceId.ThrowIfNullOrWhiteSpace(nameof(raceId));

            if (monsterTypeCatalog.ContainsKey(raceId))
            {
                return monsterTypeCatalog[raceId];
            }

            return null;
        }

        /// <summary>
        /// Finds all the entities in the set within the context that satisfy an expression.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <returns>The collection of entities retrieved.</returns>
        public IEnumerable<MonsterTypeEntity> FindMany(Expression<Func<MonsterTypeEntity, bool>> predicate)
        {
            return monsterTypeCatalog.Values.AsQueryable().Where(predicate);
        }

        /// <summary>
        /// Finds an entity in the set within the context that satisfies an expression.
        /// If more than one entity satisfies the expression, one is picked up in an unknown criteria.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <param name="includeProperties">Optional. Any additional properties to include.</param>
        /// <returns>The entity found.</returns>
        public MonsterTypeEntity FindOne(Expression<Func<MonsterTypeEntity, bool>> predicate, params string[] includeProperties)
        {
            return monsterTypeCatalog.Values.AsQueryable().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets all the entities from the set in the context.
        /// </summary>
        /// <returns>The collection of entities retrieved.</returns>
        public Task<IEnumerable<MonsterTypeEntity>> GetAll()
        {
            return Task.FromResult(monsterTypeCatalog.Values.AsEnumerable());
        }

        /// <summary>
        /// Gets an entity by the primary key, from the context.
        /// </summary>
        /// <param name="keyMembersFunc">A function that returns the keys used to find the entity, in order.</param>
        /// <returns>The entity found, if any.</returns>
        public MonsterTypeEntity GetByPrimaryKey(Func<object[]> keyMembersFunc)
        {
            keyMembersFunc.ThrowIfNull(nameof(keyMembersFunc));

            var key = keyMembersFunc();
            var raceId = key.FirstOrDefault()?.ToString();

            return GetByRaceId(raceId);
        }
    }
}
