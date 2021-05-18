// -----------------------------------------------------------------
// <copyright file="OperationCategory.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Enumerations
{
    /// <summary>
    /// Enumerates the different categories of operations in the game.
    /// </summary>
    public enum OperationCategory
    {
        /// <summary>
        /// A default category that is used to signal "any or all" operations.
        /// </summary>
        Any,

        /// <summary>
        /// Operations that involve moving a thing.
        /// </summary>
        Movement,
    }
}
