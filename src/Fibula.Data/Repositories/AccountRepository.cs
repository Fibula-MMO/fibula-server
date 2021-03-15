// -----------------------------------------------------------------
// <copyright file="AccountRepository.cs" company="2Dudes">
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
    using Fibula.Data.Contracts.Abstractions.Repositories;
    using Fibula.Definitions.Data.Entities;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Class that represents an accounts repository.
    /// </summary>
    public class AccountRepository : GenericRepository<AccountEntity>, IAccountsRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountRepository"/> class.
        /// </summary>
        /// <param name="context">The context to initialize the repository with.</param>
        public AccountRepository(DbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Attempts to find an account in the repo with a given number.
        /// </summary>
        /// <param name="number">The number of the account.</param>
        /// <returns>The entity for the account if one was bound, and null otherwise.</returns>
        /// <remarks>Does not load nested relationships, only those at this entity level.</remarks>
        public AccountEntity FindAccountByNumber(uint number)
        {
            return this.FindOne(a => a.Number == number, nameof(AccountEntity.Characters));
        }
    }
}
