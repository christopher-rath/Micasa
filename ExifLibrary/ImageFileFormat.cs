#region Copyright
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
    /// Represents the format of the <see cref="ImageFile"/>.
    /// </summary>
    public enum ImageFileFormat
    {
        /// <summary>
        /// The file is not recognized.
        /// </summary>
        Unknown,
        /// <summary>
        /// The file is a JPEG/Exif or JPEG/JFIF file.
        /// </summary>
        JPEG,
        /// <summary>
        /// The file is a TIFF File.
        /// </summary>
        TIFF,
        /// <summary>
        /// The file is a PNG File.
        /// </summary>
        PNG,
        /// <summary>
        /// The file is a GIF File.
        /// </summary>
        GIF,
    }
}
