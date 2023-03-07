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
    /// Represents the memory view of a JPEG section.
    /// A JPEG section is the data between markers of the JPEG file.
    /// </summary>
    public class JPEGSection
    {
        #region Properties
        /// <summary>
        /// The marker byte representing the section.
        /// </summary>
        public JPEGMarker Marker { get; private set; }
        /// <summary>
        /// Section header as a byte array. This is different from the header
        /// definition in JPEG specification in that it does not include the 
        /// two byte section length.
        /// </summary>
        public byte[] Header { get; set; }
        /// <summary>
        /// For the SOS and RST markers, this contains the entropy coded data.
        /// </summary>
        public byte[] EntropyData { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a JPEGSection represented by the marker byte and containing
        /// the given data.
        /// </summary>
        /// <param name="marker">The marker byte representing the section.</param>
        /// <param name="data">Section data.</param>
        /// <param name="entropydata">Entropy coded data.</param>
        public JPEGSection(JPEGMarker marker, byte[] data, byte[] entropydata)
        {
            Marker = marker;
            Header = data;
            EntropyData = entropydata;
        }

        /// <summary>
        /// Constructs a JPEGSection represented by the marker byte.
        /// </summary>
        /// <param name="marker">The marker byte representing the section.</param>
        public JPEGSection(JPEGMarker marker)
            : this(marker, new byte[0], new byte[0])
        {
            ;
        }
        #endregion

        #region Instance Methods
        /// <summary>
        /// Returns a string representation of the current section.
        /// </summary>
        /// <returns>A System.String that represents the current section.</returns>
        public override string ToString()
        {
            return string.Format("{0} => Header: {1} bytes, Entropy Data: {2} bytes", Marker, Header.Length, EntropyData.Length);
        }
        #endregion
    }
}
