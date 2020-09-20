// -----------------------------------------------------------------
// <copyright file="IScriptLoader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Scripting.Contracts.Abstractions
{
    using System;
    using Microsoft.CodeAnalysis.Scripting;

    /// <summary>
    /// Interface for script loaders.
    /// </summary>
    public interface IScriptLoader
    {
        /// <summary>
        /// Attempts to load a script from the given file path.
        /// </summary>
        /// <param name="filePath">The path to the script file.</param>
        /// <param name="scriptGlobalsType">The type of globals parameters used in the script.</param>
        /// <returns>A compiled script instance if the script file is found and loaded successfully, and null otherwise.</returns>
        Script<object> LoadScriptFromFile(string filePath, Type scriptGlobalsType);

        /// <summary>
        /// Attempts to load an inline script.
        /// </summary>
        /// <param name="scriptContent">The script content.</param>
        /// <param name="scriptGlobalsType">The type of globals parameters used in the script.</param>
        /// <returns>A compiled script instance if the script loaded successfully, and null otherwise.</returns>
        Script<object> LoadScriptInline(string scriptContent, Type scriptGlobalsType);
    }
}
