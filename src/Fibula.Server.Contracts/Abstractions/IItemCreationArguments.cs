// -----------------------------------------------------------------
// <copyright file="IItemCreationArguments.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Abstractions
{
    using System;
    using System.Collections.Generic;
    using Fibula.Definitions.Enumerations;

    /// <summary>
    /// Interface for arguments for item creation.
    /// </summary>
    public interface IItemCreationArguments
    {
        /// <summary>
        /// Gets the id of type for the item to create.
        /// </summary>
        ushort TypeId { get; }

        /// <summary>
        /// Gets the attributes to set in the item to create.
        /// </summary>
        IReadOnlyCollection<(ItemAttribute, IConvertible)> Attributes { get; }
    }
}
