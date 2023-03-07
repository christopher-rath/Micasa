﻿using System;
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
    /// Represents the type code defined in the Exif standard.
    /// </summary>
    public enum InterOpType : ushort
    {
        /// <summary>
        /// BYTE (byte)
        /// </summary>
        BYTE = 1,
        /// <summary>
        /// ASCII (byte array)
        /// </summary>
        ASCII = 2,
        /// <summary>
        /// SHORT (ushort)
        /// </summary>
        SHORT = 3,
        /// <summary>
        /// LONG (uint)
        /// </summary>
        LONG = 4,
        /// <summary>
        /// RATIONAL (2 x uint: numerator, denominator)
        /// </summary>
        RATIONAL = 5,
        /// <summary>
        /// BYTE (sbyte)
        /// </summary>
        SBYTE = 6,
        /// <summary>
        /// UNDEFINED (byte array)
        /// </summary>
        UNDEFINED = 7,
        /// <summary>
        /// SSHORT (short)
        /// </summary>
        SSHORT = 8,
        /// <summary>
        /// SLONG (int)
        /// </summary>
        SLONG = 9,
        /// <summary>
        /// SRATIONAL (2 x int: numerator, denominator)
        /// </summary>
        SRATIONAL = 10,
        /// <summary>
        /// FLOAT (float)
        /// </summary>
        FLOAT = 11,
        /// <summary>
        /// DOUBLE (double)
        /// </summary>
        DOUBLE = 12
    }
    /// <summary>
    /// Represents interoperability data for an exif tag in the platform byte order.
    /// </summary>
    public struct ExifInterOperability
    {
        private ushort mTagID;
        private InterOpType mTypeID;
        private uint mCount;
        private byte[] mData;

        /// <summary>
        /// Gets the tag ID defined in the Exif standard.
        /// </summary>
        public ushort TagID { get { return mTagID; } }
        /// <summary>
        /// Gets the type code defined in the Exif standard.
        /// </summary>
        public InterOpType TypeID { get { return mTypeID; } }
        /// <summary>
        /// Gets the byte count or number of components.
        /// </summary>
        public uint Count { get { return mCount; } }
        /// <summary>
        /// Gets the field value as an array of bytes.
        /// </summary>
        public byte[] Data { get { return mData; } }
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Tag: {0}, Type: {1}, Count: {2}, Data Length: {3}", mTagID, (ushort)mTypeID, mCount, mData.Length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifInterOperability"/> class.
        /// </summary>
        /// <param name="tagid">The Exif tag ID.</param>
        /// <param name="typeid">The Exif data type.</param>
        /// <param name="count">Count of data.</param>
        /// <param name="data">Field data as a byte array.</param>
        public ExifInterOperability(ushort tagid, InterOpType typeid, uint count, byte[] data)
        {
            mTagID = tagid;
            mTypeID = typeid;
            mCount = count;
            mData = data;
        }
    }
}
