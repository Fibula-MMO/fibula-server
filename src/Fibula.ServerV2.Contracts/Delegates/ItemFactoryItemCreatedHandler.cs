// -----------------------------------------------------------------
// <copyright file="ItemFactoryItemCreatedHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.ServerV2.Contracts.Delegates
{
    using Fibula.ServerV2.Contracts.Abstractions;

    /// <summary>
    /// Delegate to handle an item creation.
    /// </summary>
    /// <param name="item">The item created.</param>
    public delegate void ItemFactoryItemCreatedHandler(IItem item);
}
