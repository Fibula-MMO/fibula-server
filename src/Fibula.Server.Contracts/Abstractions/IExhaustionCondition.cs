﻿// -----------------------------------------------------------------
// <copyright file="IExhaustionCondition.cs" company="2Dudes">
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
    using Fibula.Definitions.Flags;

    /// <summary>
    /// Interface for the special type of exhaustion condition.
    /// </summary>
    public interface IExhaustionCondition : ICondition
    {
        /// <summary>
        /// Gets the current types that this exhaustion covers.
        /// </summary>
        IDictionary<ExhaustionFlag, DateTimeOffset> ExhaustionTimesPerType { get; }
    }
}
