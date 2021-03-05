// -----------------------------------------------------------------
// <copyright file="IOperation.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Mechanics.Contracts.Abstractions
{
    using System;
    using System.Collections.Generic;
    using Fibula.Definitions.Flags;
    using Fibula.Scheduling.Contracts.Abstractions;

    /// <summary>
    /// Interface for a game operation.
    /// </summary>
    public interface IOperation : IEvent
    {
        /// <summary>
        /// Gets the exhaustion conditions that this operation checks for and produces.
        /// </summary>
        IDictionary<ExhaustionFlag, TimeSpan> ExhaustionInfo { get; }
    }
}
