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
