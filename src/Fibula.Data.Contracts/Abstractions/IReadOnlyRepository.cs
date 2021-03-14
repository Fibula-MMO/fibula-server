// -----------------------------------------------------------------
// <copyright file="IReadOnlyRepository.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Contracts.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fibula.Data.Entities.Contracts.Abstractions;

    /// <summary>
    /// Interface for a generic, read-only entity repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IReadOnlyRepository<TEntity>
        where TEntity : IEntity
    {
        /// <summary>
        /// Gets an entity by the primary key, from the context.
        /// </summary>
        /// <param name="keyMembersFunc">A function that returns the keys used to find the entity, in order.</param>
        /// <returns>The entity found, if any.</returns>
        TEntity GetByPrimaryKey(Func<object[]> keyMembersFunc);

        /// <summary>
        /// Gets all the entities from the set in the context.
        /// </summary>
        /// <returns>The collection of entities retrieved.</returns>
        Task<IEnumerable<TEntity>> GetAll();

        /// <summary>
        /// Finds all entities that match a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to use for matching.</param>
        /// <returns>The entities that matched the predicate.</returns>
        IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Finds an entity in the set within the context that satisfies an expression.
        /// If more than one entity satisfies the expression, one is picked up in an unknown criteria.
        /// </summary>
        /// <param name="predicate">The expression to satisfy.</param>
        /// <returns>The entity found.</returns>
        TEntity FindOne(Expression<Func<TEntity, bool>> predicate);
    }
}
