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
    /// Represents a TIFF Header.
    /// </summary>
    public struct TIFFHeader
    {
        /// <summary>
        /// The byte order of the image file.
        /// </summary>
        public BitConverterEx.ByteOrder ByteOrder;
        /// <summary>
        /// TIFF ID. This value should always be 42.
        /// </summary>
        public byte ID;
        /// <summary>
        /// The offset to the first IFD section from the 
        /// start of the TIFF header.
        /// </summary>
        public uint IFDOffset;
        /// <summary>
        /// The byte order of the TIFF header itself.
        /// </summary>
        public BitConverterEx.ByteOrder TIFFHeaderByteOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TIFFHeader"/> struct.
        /// </summary>
        /// <param name="byteOrder">The byte order.</param>
        /// <param name="id">The TIFF ID. This value should always be 42.</param>
        /// <param name="ifdOffset">The offset to the first IFD section from the 
        /// start of the TIFF header.</param>
        /// <param name="headerByteOrder">The byte order of the TIFF header itself.</param>
        public TIFFHeader(BitConverterEx.ByteOrder byteOrder, byte id, uint ifdOffset, BitConverterEx.ByteOrder headerByteOrder)
        {
            if (id != 42)
                throw new NotValidTIFFHeader();

            ByteOrder = byteOrder;
            ID = id;
            IFDOffset = ifdOffset;
            TIFFHeaderByteOrder = headerByteOrder;
        }

        /// <summary>
        /// Returns a <see cref="TIFFHeader"/> initialized from the given byte data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset into <paramref name="data"/>.</param>
        /// <returns>A <see cref="TIFFHeader"/> initialized from the given byte data.</returns>
        public static TIFFHeader FromBytes(byte[] data, int offset)
        {
            TIFFHeader header = new TIFFHeader();

            // TIFF header
            if (data[offset] == 0x49 && data[offset + 1] == 0x49)
                header.ByteOrder = BitConverterEx.ByteOrder.LittleEndian;
            else if (data[offset] == 0x4D && data[offset + 1] == 0x4D)
                header.ByteOrder = BitConverterEx.ByteOrder.BigEndian;
            else
                throw new NotValidTIFFHeader();

            // TIFF header may have a different byte order
            if (BitConverterEx.LittleEndian.ToUInt16(data, offset + 2) == 42)
                header.TIFFHeaderByteOrder = BitConverterEx.ByteOrder.LittleEndian;
            else if (BitConverterEx.BigEndian.ToUInt16(data, offset + 2) == 42)
                header.TIFFHeaderByteOrder = BitConverterEx.ByteOrder.BigEndian;
            else
                throw new NotValidTIFFHeader();
            header.ID = 42;

            // IFD offset
            header.IFDOffset = BitConverterEx.ToUInt32(data, offset + 4, header.TIFFHeaderByteOrder, BitConverterEx.SystemByteOrder);

            return header;
        }
    }
}
