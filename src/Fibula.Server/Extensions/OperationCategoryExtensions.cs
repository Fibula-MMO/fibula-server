// -----------------------------------------------------------------
// <copyright file="OperationCategoryExtensions.cs" company="2Dudes">
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
    using System;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Enumerations;
    using Fibula.Server.Mechanics.Operations;

    /// <summary>
    /// Static class that contains extension methods related to the <see cref="OperationCategory"/> enumeration.
    /// </summary>
    public static class OperationCategoryExtensions
    {
        /// <summary>
        /// Converts a given <see cref="OperationCategory"/> to it's equivalent in-game operation <see cref="Type"/>.
        /// </summary>
        /// <param name="category">The category to convert.</param>
        /// <returns>The type deemed equivaled to this category.</returns>
        public static Type ToOperationType(this OperationCategory category)
        {
            return category switch
            {
                OperationCategory.Movement => typeof(MovementOperation),
                _ => typeof(IOperation)
            };
        }
    }
}
