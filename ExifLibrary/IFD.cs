﻿#region Copyright
// ExifLibrary - a .Net Standard library for editing Exif metadata contained in image files.
// Author: Özgür Özçıtak
// Based-on Version: 2.1.4
// Updates by: Christopher Rath <christopher@rath.ca>
// Archived at: https://oozcitak.github.io/exiflibrary/
// Copyright (c) 2013 Özgür Özçıtak
// Distributed under the MIT License (MIT) -- see http://opensource.org/licenses/MIT
// Warranty: None, see the license.
#endregion

namespace ExifLibrary
{
    /// <summary>
    /// Represents the IFD section containing tags.
    /// </summary>
    public enum IFD : int
    {
        /// <summary>
        /// Unkown IFD section.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Zeroth IFD section.
        /// </summary>
        Zeroth = 100000,
        /// <summary>
        /// Exif IFD section.
        /// </summary>
        EXIF = 200000,
        /// <summary>
        /// GPS IFD section.
        /// </summary>
        GPS = 300000,
        /// <summary>
        /// Interop IFD section.
        /// </summary>
        Interop = 400000,
        /// <summary>
        /// First IFD section.
        /// </summary>
        First = 500000,
        /// <summary>
        /// A pseudo-IFD section containing makernotes.
        /// </summary>
        MakerNote = 600000,
        /// <summary>
        /// A pseudo-IFD section containing JFIF tags.
        /// </summary>
        JFIF = 700000,
        /// <summary>
        /// A pseudo-IFD section containing JFXX tags.
        /// </summary>
        JFXX = 800000,
        /// <summary>
        /// A pseudo-IFD section containing PGN tags.
        /// </summary>
        PNG = 900000,
        /// <summary>
        /// A pseudo-IFD section containing GIF tags.
        /// </summary>
        GIF = 1000000,
    }
}
