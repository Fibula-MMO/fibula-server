// -----------------------------------------------------------------
// <copyright file="MapDescriptionType.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Server.Contracts.Enumerations
{
    /// <summary>
    /// Enumerates the different types of map description.
    /// </summary>
    public enum MapDescriptionType
    {
        /// <summary>
        /// A full description around a particular location.
        /// </summary>
        Full,

        /// <summary>
        /// A partial description for the left and top slices only.
        /// </summary>
        LeftAndTop,

        /// <summary>
        /// A partial description for the bottom and right slices only.
        /// </summary>
        BottomAndRight,

        /// <summary>
        /// A partial description for the left slice only.
        /// </summary>
        LeftOnly,

        /// <summary>
        /// A partial description for the right slice only.
        /// </summary>
        RightOnly,

        /// <summary>
        /// A partial description for the top slice only.
        /// </summary>
        TopOnly,

        /// <summary>
        /// A partial description for the bottom slice only.
        /// </summary>
        BottomOnly,
    }
}
