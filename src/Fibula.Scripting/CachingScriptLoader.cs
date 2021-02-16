// -----------------------------------------------------------------
// <copyright file="CachingScriptLoader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Scripting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Fibula.Scripting.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;
    using Serilog;

    /// <summary>
    /// Class that implements a script loader that caches the loaded scripts.
    /// </summary>
    public class CachingScriptLoader : IScriptLoader
    {
        /// <summary>
        /// Stores the referenece to the logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The mapping of script file paths to their compiled scripts.
        /// </summary>
        private readonly IDictionary<string, Script<object>> compiledMap;

        /// <summary>
        /// The object used as a lock to sempahore access to the <see cref="compiledMap"/>.
        /// </summary>
        private readonly object compiledScriptsLock;

        /// <summary>
        /// Stores the scripting options used then invoking scripts.
        /// </summary>
        private ScriptOptions scriptOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingScriptLoader"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        public CachingScriptLoader(ILogger logger)
        {
            logger.ThrowIfNull(nameof(logger));

            this.logger = logger.ForContext<CachingScriptLoader>();

            this.compiledMap = new Dictionary<string, Script<object>>();
            this.compiledScriptsLock = new object();

            this.InitializeScripting();
        }

        /// <summary>
        /// Attempts to load an inline script.
        /// </summary>
        /// <param name="scriptContent">The script content.</param>
        /// <param name="scriptGlobalsType">The type of globals parameters used in the script.</param>
        /// <returns>A compiled script instance if the script loaded successfully, and null otherwise.</returns>
        public Script<object> LoadScriptInline(string scriptContent, Type scriptGlobalsType)
        {
            if (string.IsNullOrWhiteSpace(scriptContent) || scriptGlobalsType == null)
            {
                return null;
            }

            if (!this.compiledMap.TryGetValue(scriptContent, out Script<object> script))
            {
                lock (this.compiledScriptsLock)
                {
                    if (!this.compiledMap.TryGetValue(scriptContent, out script))
                    {
                        script = CSharpScript.Create<object>(scriptContent, this.scriptOptions, scriptGlobalsType);

                        script.Compile();

                        this.compiledMap.Add(scriptContent, script);
                    }
                }
            }

            return script;
        }

        /// <summary>
        /// Attempts to load a script from the given file path.
        /// </summary>
        /// <param name="filePath">The path to the script file.</param>
        /// <param name="scriptGlobalsType">The type of globals parameters used in the script.</param>
        /// <returns>A compiled script instance if the script file is found and loaded successfully, and null otherwise.</returns>
        public Script<object> LoadScriptFromFile(string filePath, Type scriptGlobalsType)
        {
            filePath.ThrowIfNullOrWhiteSpace(nameof(filePath));
            scriptGlobalsType.ThrowIfNull(nameof(scriptGlobalsType));

            if (!File.Exists(filePath))
            {
                this.logger.Debug($"No script file '{filePath}' found.");

                return null;
            }

            if (!this.compiledMap.TryGetValue(filePath, out Script<object> script))
            {
                lock (this.compiledScriptsLock)
                {
                    if (!this.compiledMap.TryGetValue(filePath, out script))
                    {
                        using var fs = File.OpenRead(filePath);

                        script = CSharpScript.Create<object>(fs, this.scriptOptions, scriptGlobalsType);

                        script.Compile();

                        this.compiledMap.Add(filePath, script);
                    }
                }
            }

            return script;
        }

        private void InitializeScripting()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();

            try
            {
                this.scriptOptions = ScriptOptions.Default.WithReferences(asms);
            }
            catch (Exception ex)
            {
                this.logger.Warning(ex.ToString());
            }
        }
    }
}
