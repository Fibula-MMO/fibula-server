// -----------------------------------------------------------------
// <copyright file="GenericReadOnlyRepository.cs" company="2Dudes">
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
    using Fibula.Data.Entities;
    using Fibula.Utilities.Validation;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Class that represents a read-only repository for an entity in the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    public abstract class GenericReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericReadOnlyRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="context">The context to use with this repository.</param>
        public GenericReadOnlyRepository(DbContext context)
        {
            context.ThrowIfNull(nameof(context));

            this.Context = context;
        }

        /// <summary>
        /// Gets the reference to the context.
        /// </summary>
        protected DbContext Context { get; }

        /// <summary>
        /// Gets an entity by the primary key, from the context.
        /// </summary>
        /// <param name="keyMembersFunc">A function that returns the keys used to find the entity, in order.</param>
        /// <returns>The entity found, if any.</returns>
        public TEntity GetByPrimaryKey(Func<object[]> keyMembersFunc)
        {
            return this.Context.Set<TEntity>().Find(keyMembersFunc());
        }

        /// <summary>
        /// Gets all the entities from the set in the context.
        /// </summary>
        /// <returns>The collection of entities retrieved.</returns>
        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await this.Context.Set<TEntity>().ToListAsync();
        }

        /// <summary>
        /// Finds all the entities in the set within the context that satisfy an expression.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <returns>The collection of entities retrieved.</returns>
        public IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> predicate)
        {
            return this.Context.Set<TEntity>().Where(predicate);
        }

        /// <summary>
        /// Finds an entity in the set within the context that satisfies an expression.
        /// If more than one entity satisfies the expression, one is picked up in an unknown criteria.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <returns>The entity found.</returns>
        public TEntity FindOne(Expression<Func<TEntity, bool>> predicate)
        {
            return this.Context.Set<TEntity>().FirstOrDefault(predicate);
        }
    }
}
