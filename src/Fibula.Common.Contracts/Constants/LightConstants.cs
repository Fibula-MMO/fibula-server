// -----------------------------------------------------------------
// <copyright file="LightConstants.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Common.Contracts.Constants
{
    /// <summary>
    /// Static class that containts constants related to light.
    /// </summary>
    public static class LightConstants
    {
        /// <summary>
        /// No color or no light at all.
        /// </summary>
        public const byte None = 0x00;

        /// <summary>
        /// The default color, which is <see cref="OrangeColor"/>.
        /// </summary>
        public const byte DefaultColor = LightConstants.OrangeColor;

        /// <summary>
        /// The code for orange colored light.
        /// </summary>
        public const byte OrangeColor = 206;

        /// <summary>
        /// The code for white light.
        /// </summary>
        public const byte WhiteColor = 215;

        /// <summary>
        /// The light level emitted by a brand-new torch.
        /// </summary>
        public const byte TorchLevel = 7;

        /// <summary>
        /// The level considered as full.
        /// </summary>
        public const byte FullLight = 27;

        /// <summary>
        /// The world light level considered as night.
        /// </summary>
        public const int WorldNight = 30;

        /// <summary>
        /// The world light level considered as dusk/dawn.
        /// </summary>
        public const int WorldDuskDawn = 130;

        /// <summary>
        /// The world light level considered as day.
        /// </summary>
        public const byte WorldDay = 255;
    }
}
