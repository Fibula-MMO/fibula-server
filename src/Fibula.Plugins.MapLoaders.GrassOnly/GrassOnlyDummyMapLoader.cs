// -----------------------------------------------------------------
// <copyright file="GrassOnlyDummyMapLoader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

#pragma warning disable 67
namespace Fibula.Plugins.MapLoaders.GrassOnly
{
    using System.Collections.Generic;
    using System.Linq;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Server.Contracts.Abstractions;
    using Fibula.Server.Contracts.Delegates;
    using Fibula.Server.Contracts.Models;
    using Fibula.Utilities.Validation;

    /// <summary>
    /// Class that represents a dummy map loader that yields all grass tiles.
    /// </summary>
    public sealed class GrassOnlyDummyMapLoader : IMapLoader
    {
        /// <summary>
        /// The id of the type for a grass tile.
        /// </summary>
        private const int GrassTypeId = 102;

        /// <summary>
        /// The item factory instance.
        /// </summary>
        private readonly IItemFactory itemFactory;

        /// <summary>
        /// The tile factory instance.
        /// </summary>
        private readonly ITileFactory tileFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrassOnlyDummyMapLoader"/> class.
        /// </summary>
        /// <param name="itemFactory">A reference to the item factory.</param>
        /// <param name="tileFactory">A reference to the tile factory.</param>
        public GrassOnlyDummyMapLoader(IItemFactory itemFactory, ITileFactory tileFactory)
        {
            itemFactory.ThrowIfNull(nameof(itemFactory));
            tileFactory.ThrowIfNull(nameof(tileFactory));

            this.itemFactory = itemFactory;
            this.tileFactory = tileFactory;
        }

        /// <summary>
        /// Event not in use for this loader.
        /// </summary>
        public event MapWindowLoadedHandler WindowLoaded;

        /// <summary>
        /// Gets the percentage completed loading the map [0, 100].
        /// </summary>
        public byte PercentageComplete => 0x64;

        /// <summary>
        /// Computes a hash string using the given window parameters.
        /// </summary>
        /// <param name="dimensions">The dimensions to compute the hash with.</param>
        /// <returns>A hash represented by a string.</returns>
        public string GetLoadHash(IMapWindowDimensions dimensions)
        {
            return $"{dimensions.FromX}:{dimensions.FromY}:{dimensions.FromZ}";
        }

        /// <summary>
        /// Attempts to load all tiles within a map window.
        /// </summary>
        /// <param name="window">The parameters to for the window to load.</param>
        /// <returns>A collection of <see cref="ITile"/>s loaded.</returns>
        public IEnumerable<ITile> Load(IMapWindowDimensions window)
        {
            window.ThrowIfNull(nameof(window));

            if (window.FromZ != 7)
            {
                return Enumerable.Empty<ITile>();
            }

            var tiles = new List<ITile>();

            for (int x = window.FromX; x <= window.ToX; x++)
            {
                for (int y = window.FromY; y <= window.ToY; y++)
                {
                    var groundItem = this.itemFactory.CreateItem(ItemCreationArguments.WithTypeId(GrassTypeId));

                    var location = new Location() { X = x, Y = y, Z = window.FromZ };
                    var newTile = this.tileFactory.CreateTile(location, groundItem);

                    tiles.Add(newTile);
                }
            }

            return tiles;
        }
    }
}
#pragma warning restore 67
