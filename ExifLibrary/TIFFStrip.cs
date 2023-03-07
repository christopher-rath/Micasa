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
