// -----------------------------------------------------------------
// <copyright file="StatPercentChangedHandler.cs" company="2Dudes">
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
    /// Delegate meant for a stat changes in percentual value.
    /// </summary>
    /// <param name="statType">The type of the stat that changed.</param>
    /// <param name="previousPercent">The previous percentual value of the stat.</param>
    public delegate void StatPercentChangedHandler(CreatureStat statType, byte previousPercent);
}
