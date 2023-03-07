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
    /// Represents the units for the X and Y densities
    /// for a JFIF file.
    /// </summary>
    public enum JFIFDensityUnit : byte
    {
        /// <summary>
        /// No units, XDensity and YDensity specify the pixel aspect ratio.
        /// </summary>
        None = 0,
        /// <summary>
        /// XDensity and YDensity are dots per inch.
        /// </summary>
        DotsPerInch = 1,
        /// <summary>
        /// XDensity and YDensity are dots per cm.
        /// </summary>
        DotsPerCm = 2,
    }
    /// <summary>
    /// Represents the JFIF extension.
    /// </summary>
    public enum JFIFExtension : byte
    {
        /// <summary>
        /// Thumbnail coded using JPEG.
        /// </summary>
        ThumbnailJPEG = 0x10,
        /// <summary>
        /// Thumbnail stored using a 256-Color RGB palette.
        /// </summary>
        ThumbnailPaletteRGB = 0x11,
        /// <summary>
        /// Thumbnail stored using 3 bytes/pixel (24-bit) RGB values.
        /// </summary>
        Thumbnail24BitRGB = 0x13,
    }
}
