// -----------------------------------------------------------------
// <copyright file="StatValueChangedHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Delegates
{
    using Fibula.Server.Contracts.Enumerations;

    /// <summary>
    /// Delegate meant for a stat changes in value.
    /// </summary>
    /// <param name="statType">The type of the stat that changed.</param>
    /// <param name="previousValue">The previous stat value.</param>
    public delegate void StatValueChangedHandler(CreatureStat statType, uint previousValue);
}
