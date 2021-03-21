// -----------------------------------------------------------------
// <copyright file="GameHandler.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Mechanics.Handlers
{
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class that represents a handler that exposes a reference to the game instance, for convenience.
    /// </summary>
    public abstract class GameHandler : BaseRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameHandler"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger in use.</param>
        /// <param name="game">A reference to the game instance.</param>
        protected GameHandler(ILogger logger, IGame game)
            : base(logger)
        {
            game.ThrowIfNull(nameof(game));

            this.Game = game;
        }

        /// <summary>
        /// Gets the reference to the game instance.
        /// </summary>
        protected IGame Game { get; }
    }
}
