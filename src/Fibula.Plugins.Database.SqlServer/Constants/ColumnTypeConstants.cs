// -----------------------------------------------------------------
// <copyright file="ColumnTypeConstants.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Plugins.Database.SqlServer.Constants
{
    /// <summary>
    /// Static class that contains constants for column types, mainly to avoid typos.
    /// </summary>
    public static class ColumnTypeConstants
    {
        /// <summary>
        /// The column type for an <see cref="int"/> field.
        /// </summary>
        public const string Int32 = "int";

        /// <summary>
        /// The column type for an <see cref="uint"/> field.
        /// </summary>
        public const string UInt32 = "int";

        /// <summary>
        /// The column type for a <see cref="short"/> field.
        /// </summary>
        public const string Int16 = "smallint";

        /// <summary>
        /// The column type for a <see cref="byte"/> field.
        /// </summary>
        public const string Byte = "tinyint";

        /// <summary>
        /// The column type for a <see cref="DateTimeOffset"/> field.
        /// </summary>
        public const string DateTimeOffset = "datetimeoffset(0)";
    }
}
