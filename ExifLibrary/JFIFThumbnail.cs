using System;
using System.Collections.Generic;
using System.Text;

namespace ExifLibrary
{
#pragma warning disable CA1036 // Override methods on comparable types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable CA1507 // Use nameof to express symbol names
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
#pragma warning disable CA1715 // Identifiers should have correct prefix
#pragma warning disable CA1720 // Identifier contains type name
#pragma warning disable CA1725 // Parameter names should match base declaration
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1825 // Avoid zero-length array allocations
#pragma warning disable CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method
#pragma warning disable CA2251 // Use 'string.Equals'
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE1006 // Naming Styles
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
