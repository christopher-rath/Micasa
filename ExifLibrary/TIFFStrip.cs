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
using System;
using System.Collections.Generic;
using System.Text;

namespace ExifLibrary
{
    /// <summary>
    /// Represents a strip of compressed image data in a TIFF file.
    /// </summary>
    public class TIFFStrip
    {
        /// <summary>
        /// Compressed image data contained in this strip.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TIFFStrip"/> class.
        /// </summary>
        /// <param name="data">The byte array to copy strip from.</param>
        /// <param name="offset">The offset to the beginning of strip.</param>
        /// <param name="length">The length of strip.</param>
        public TIFFStrip(byte[] data, uint offset, uint length)
        {
            Data = new byte[length];
            Array.Copy(data, (int)offset, Data, 0, (int)length);
        }
    }
}
