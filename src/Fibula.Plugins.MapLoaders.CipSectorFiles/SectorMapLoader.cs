// -----------------------------------------------------------------
// <copyright file="SectorMapLoader.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.MapLoaders.CipSectorFiles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fibula.Definitions.Data.Structures;
    using Fibula.Definitions.Enumerations;
    using Fibula.Parsing.CipFiles;
    using Fibula.Parsing.CipFiles.Enumerations;
    using Fibula.Parsing.CipFiles.Extensions;
    using Fibula.Parsing.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Abstractions;
    using Fibula.ServerV2.Contracts.Enumerations;
    using Fibula.ServerV2.Contracts.Models;
    using Fibula.Utilities.Validation;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class that represents a map loader that loads from the sector files from CipSoft.
    /// </summary>
    public sealed class SectorMapLoader : IMapLoader
    {
        /// <summary>
        /// The symbol used for comments.
        /// </summary>
        public const char CommentSymbol = '#';

        /// <summary>
        /// The separator used for sector files.
        /// </summary>
        public const char SectorSeparator = ':';

        /// <summary>
        /// The separator used for positions of sectors.
        /// </summary>
        public const char PositionSeparator = '-';

        /// <summary>
        /// The minimum X value for sectors known.
        /// </summary>
        public const int SectorXMin = 996;

        /// <summary>
        /// The maximum X value for sectors known.
        /// </summary>
        public const int SectorXMax = 1043;

        /// <summary>
        /// The minimum Y value for sectors known.
        /// </summary>
        public const int SectorYMin = 984;

        /// <summary>
        /// The maximum Y value for sectors known.
        /// </summary>
        public const int SectorYMax = 1031;

        /// <summary>
        /// The minimum Z value for sectors known.
        /// </summary>
        public const int SectorZMin = 0;

        /// <summary>
        /// The maximum Z value for sectors known.
        /// </summary>
        public const int SectorZMax = 15;

        /// <summary>
        /// The size of a square Sector.
        /// </summary>
        private const int SquareSectorSize = 32;

        /// <summary>
        /// The logger to use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The item factory instance.
        /// </summary>
        private readonly IItemFactory itemFactory;

        /// <summary>
        /// The tile factory instance.
        /// </summary>
        private readonly ITileFactory tileFactory;

        /// <summary>
        /// Holds the map directory info.
        /// </summary>
        private readonly DirectoryInfo mapDirInfo;

        /// <summary>
        /// The Z length of sectors known.
        /// </summary>
        private readonly int sectorsLengthZ;

        /// <summary>
        /// The Y length of sectors known.
        /// </summary>
        private readonly int sectorsLengthY;

        /// <summary>
        /// The X length of sectors known.
        /// </summary>
        private readonly int sectorsLengthX;

        /// <summary>
        /// Holds the loaded sectors.
        /// </summary>
        private readonly bool[,,] sectorsLoaded;

        /// <summary>
        /// An object used as a lock to semaphore loading into the sectors' dictionary.
        /// </summary>
        private readonly object loadLock;

        /// <summary>
        /// The total known tile count.
        /// </summary>
        private long totalTileCount;

        /// <summary>
        /// The total loaded tiles count.
        /// </summary>
        private long totalLoadedCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectorMapLoader"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger instance in use.</param>
        /// <param name="itemFactory">A reference to the item factory.</param>
        /// <param name="tileFactory">A reference to the tile factory.</param>
        /// <param name="sectorMapLoaderOptions">The options for this map loader.</param>
        public SectorMapLoader(
            ILogger<SectorMapLoader> logger,
            IItemFactory itemFactory,
            ITileFactory tileFactory,
            IOptions<SectorMapLoaderOptions> sectorMapLoaderOptions)
        {
            logger.ThrowIfNull(nameof(logger));
            itemFactory.ThrowIfNull(nameof(itemFactory));
            tileFactory.ThrowIfNull(nameof(tileFactory));
            sectorMapLoaderOptions.ThrowIfNull(nameof(sectorMapLoaderOptions));

            DataAnnotationsValidator.ValidateObjectRecursive(sectorMapLoaderOptions.Value);

            this.mapDirInfo = new DirectoryInfo(sectorMapLoaderOptions.Value.LiveMapDirectory);

            if (!this.mapDirInfo.Exists)
            {
                throw new ApplicationException($"The map directory '{sectorMapLoaderOptions.Value.LiveMapDirectory}' could not be found.");
            }

            this.logger = logger;
            this.itemFactory = itemFactory;
            this.tileFactory = tileFactory;

            this.totalTileCount = 1;
            this.totalLoadedCount = default;

            this.loadLock = new object();

            this.sectorsLengthX = 1 + SectorXMax - SectorXMin;
            this.sectorsLengthY = 1 + SectorYMax - SectorYMin;
            this.sectorsLengthZ = 1 + SectorZMax - SectorZMin;

            this.sectorsLoaded = new bool[this.sectorsLengthX, this.sectorsLengthY, this.sectorsLengthZ];
        }

        /// <summary>
        /// Gets the percentage completed loading the map [0, 100].
        /// </summary>
        public byte PercentageComplete => (byte)Math.Floor(Math.Min(100, Math.Max(0M, this.totalLoadedCount * 100 / (this.totalTileCount + 1))));

        /// <summary>
        /// Computes a hash string using the given window parameters.
        /// </summary>
        /// <param name="dimensions">The dimensions to compute the hash with.</param>
        /// <returns>A hash represented by a string.</returns>
        public string GetLoadHash(IMapWindowDimensions dimensions)
        {
            var convertToSector = dimensions.FromX > SectorXMax;

            var probeX = convertToSector ? (dimensions.FromX / SquareSectorSize) - SectorXMin : dimensions.FromX - SectorXMin;
            var probeY = convertToSector ? (dimensions.FromY / SquareSectorSize) - SectorYMin : dimensions.FromY - SectorYMin;
            var probeZ = dimensions.FromZ - SectorZMin;

            return $"{probeX}:{probeY}:{probeZ}";
        }

        /// <summary>
        /// Attempts to load all tiles within a map window.
        /// </summary>
        /// <param name="window">The parameters to for the window to load.</param>
        /// <returns>A collection of <see cref="ITile"/>s loaded.</returns>
        public IEnumerable<ITile> Load(IMapWindowDimensions window)
        {
            var fromSectorX = window.FromX / SquareSectorSize;
            var toSectorX = window.ToX / SquareSectorSize;
            var fromSectorY = window.FromY / SquareSectorSize;
            var toSectorY = window.ToY / SquareSectorSize;

            if (toSectorX < fromSectorX || toSectorY < fromSectorY || window.ToZ < window.FromZ ||
                fromSectorX < SectorXMin || toSectorX > SectorXMax ||
                fromSectorY < SectorYMin || toSectorY > SectorYMax ||
                window.FromZ < SectorZMin || window.ToZ > SectorZMax)
            {
                throw new InvalidOperationException("Bad range supplied.");
            }

            var tiles = new List<ITile>();

            this.totalTileCount = (toSectorX - fromSectorX + 1) * SquareSectorSize * (toSectorY - fromSectorY + 1) * SquareSectorSize * (window.ToZ - window.FromZ + 1);
            this.totalLoadedCount = default;

            lock (this.loadLock)
            {
                Parallel.For(window.FromZ, window.ToZ + 1, sectorZ =>
                {
                    Parallel.For(fromSectorY, toSectorY + 1, sectorY =>
                    {
                        Parallel.For(fromSectorX, toSectorX + 1, sectorX =>
                        {
                            var sectorFileName = $"{sectorX:0000}-{sectorY:0000}-{sectorZ:00}.sec";

                            var fullFilePath = Path.Combine(this.mapDirInfo.FullName, sectorFileName);
                            var sectorFileInfo = new FileInfo(fullFilePath);

                            if (sectorFileInfo.Exists)
                            {
                                using var streamReader = sectorFileInfo.OpenText();

                                var loadedTiles = this.ReadSector(sectorFileName, streamReader.ReadToEnd(), (ushort)(sectorX * SquareSectorSize), (ushort)(sectorY * SquareSectorSize), (sbyte)sectorZ);

                                tiles.AddRange(loadedTiles);
                            }

                            // 1024 per sector file, regardless if there is a tile or not...
                            Interlocked.Add(ref this.totalLoadedCount, 1024);

                            this.sectorsLoaded[sectorX - SectorXMin, sectorY - SectorYMin, sectorZ - SectorZMin] = true;

                            this.logger.LogDebug($"Loaded sector {sectorFileName} [{this.totalLoadedCount} out of {this.totalTileCount}].");
                        });
                    });
                });
            }

            this.totalLoadedCount = this.totalTileCount;

            return tiles;
        }

        private IList<ITile> ReadSector(string fileName, string sectorFileContents, ushort xOffset, ushort yOffset, sbyte z)
        {
            var loadedTilesList = new List<ITile>();

            var lines = sectorFileContents.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var readLine in lines)
            {
                var inLine = readLine?.Split(new[] { CommentSymbol }, 2).FirstOrDefault();

                // ignore comments and empty lines.
                if (string.IsNullOrWhiteSpace(inLine))
                {
                    continue;
                }

                var data = inLine.Split(new[] { SectorSeparator }, 2);

                if (data.Length != 2)
                {
                    throw new InvalidDataException($"Malformed line [{inLine}] in sector file: [{fileName}]");
                }

                var tileInfo = data[0].Split(new[] { PositionSeparator }, 2);
                var tileData = data[1];

                var location = new Location
                {
                    X = (ushort)(xOffset + Convert.ToUInt16(tileInfo[0])),
                    Y = (ushort)(yOffset + Convert.ToUInt16(tileInfo[1])),
                    Z = z,
                };

                // start off with a tile that has no ground in it.
                ITile newTile = this.tileFactory.CreateTile(location);

                this.AddContent(newTile, CipFileParser.Parse(tileData));

                loadedTilesList.Add(newTile);
            }

            this.logger.LogTrace($"Sector file {fileName}: {loadedTilesList.Count} tiles loaded.");

            return loadedTilesList;
        }

        /// <summary>
        /// Forcefully adds parsed content elements to this container.
        /// </summary>
        /// <param name="tile">The tile to add content to.</param>
        /// <param name="contentElements">The content elements to add.</param>
        private void AddContent(ITile tile, IEnumerable<IParsedElement> contentElements)
        {
            contentElements.ThrowIfNull(nameof(contentElements));

            // load and add tile flags and contents.
            foreach (var e in contentElements)
            {
                foreach (var attribute in e.Attributes)
                {
                    if (attribute.Name.Equals("Content"))
                    {
                        if (attribute.Value is IEnumerable<IParsedElement> elements)
                        {
                            var stack = new Stack<IItem>();

                            foreach (var element in elements)
                            {
                                IItem item = this.itemFactory.CreateItem(ItemCreationArguments.WithTypeId((ushort)element.Id));

                                if (item == null)
                                {
                                    this.logger.LogWarning($"Item with id {element.Id} not found in the catalog, skipping.");

                                    continue;
                                }

                                this.SetItemAttributes(item, element.Attributes);

                                stack.Push(item);
                            }

                            // Add them in reversed order.
                            while (stack.Count > 0)
                            {
                                var item = stack.Pop();

                                tile.AddItem(this.itemFactory, item);

                                item.ParentContainer = tile;
                            }
                        }
                    }
                    else
                    {
                        // it's a flag
                        if (Enum.TryParse(attribute.Name, out TileFlag flagMatch))
                        {
                            // TODO: implement
                            // tile.SetFlag(flagMatch);
                        }
                        else
                        {
                            this.logger.LogWarning($"Unknown flag [{attribute.Name}] found on tile at location {tile.Location}.");
                        }
                    }
                }
            }
        }

        private void SetItemAttributes(IItem item, IList<IParsedAttribute> attributes)
        {
            if (attributes == null)
            {
                return;
            }

            foreach (var attribute in attributes)
            {
                // TODO: support for container items.
                /*
                if ("Content".Equals(attribute.Name) && item is IContainerItem containerItem)
                {
                    if (!(attribute.Value is IEnumerable<IParsedElement> contentElements) || !contentElements.Any())
                    {
                        continue;
                    }

                    foreach (var element in contentElements)
                    {
                        IItem contentItem = this.itemFactory.CreateItem(ItemCreationArguments.WithTypeId((ushort)element.Id));

                        if (contentItem == null)
                        {
                            this.logger.LogWarning($"Item with id {element.Id} not found in the catalog, skipping.");

                            continue;
                        }

                        this.SetItemAttributes(contentItem, element.Attributes);

                        // TODO: we should be able to go over capacity here.
                        containerItem.AddContent(this.itemFactory, contentItem);
                    }

                    continue;
                }
                */

                // These are safe to add as Attributes of the item.
                if (!Enum.TryParse(attribute.Name, out CipItemAttribute cipAttr) || !(cipAttr.ToItemAttribute() is ItemAttribute itemAttribute))
                {
                    this.logger.LogWarning($"Unsupported attribute {attribute.Name} on {item.Type.Name}, ignoring.");

                    continue;
                }

                try
                {
                    item.Attributes[itemAttribute] = attribute.Value as IConvertible;
                }
                catch
                {
                    this.logger.LogWarning($"Unexpected attribute {attribute.Name} with illegal value {attribute.Value} on item {item.Type.Name}, ignoring.");
                }
            }
        }
    }
}
