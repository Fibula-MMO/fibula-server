// -----------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0083:Use pattern matching", Justification = "Pattern matching is bugged for negated patterns such as (object is not Type castedObject).", Scope = "module")]
