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
#pragma warning disable CA1825 // Avoid zero-length array allocations
    /// <summary>
    /// Represents a JFIF thumbnail.
    /// </summary>
    public class JFIFThumbnail
    {
        #region Properties
        /// <summary>
        /// Gets the 256 color RGB palette.
        /// </summary>
        public byte[] Palette { get; private set; }
        /// <summary>
        /// Gets raw image data.
        /// </summary>
        public byte[] PixelData { get; private set; }
        /// <summary>
        /// Gets the image format.
        /// </summary>
        public ImageFormat Format { get; private set; }
        #endregion

        #region Public Enums
        public enum ImageFormat
        {
            JPEG,
            BMPPalette,
            BMP24Bit,
        }
        #endregion

        #region Constructors
        protected JFIFThumbnail()
        {
            Palette = new byte[0];
            PixelData = new byte[0];
        }

        public JFIFThumbnail(ImageFormat format, byte[] data)
            : this()
        {
            Format = format;
            PixelData = data;
        }

        public JFIFThumbnail(byte[] palette, byte[] data)
            : this()
        {
            Format = ImageFormat.BMPPalette;
            Palette = palette;
            PixelData = data;
        }
        #endregion
    }
}
