// -----------------------------------------------------------------
// <copyright file="IAccountsRepository.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Data.Contracts.Abstractions.Repositories
{
    using Fibula.Definitions.Data.Entities;

    /// <summary>
    /// Interface for a repository of accounts.
    /// </summary>
    public interface IAccountsRepository : IRepository<AccountEntity>
    {
        /// <summary>
        /// Attempts to find an account in the repo with a given number.
        /// </summary>
        /// <param name="number">The number of the account.</param>
        /// <returns>The entity for the account if one was bound, and null otherwise.</returns>
        AccountEntity FindByNumber(uint number);
    }
}
