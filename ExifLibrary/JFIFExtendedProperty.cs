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
    /// Represents the JFIF version as a 16 bit unsigned integer. (EXIF Specification: SHORT) 
    /// </summary>
    public class JFIFVersion : ExifUShort
    {
        /// <summary>
        /// Gets the major version.
        /// </summary>
        public byte Major { get { return (byte)(mValue >> 8); } }
        /// <summary>
        /// Gets the minor version.
        /// </summary>
        public byte Minor { get { return (byte)(mValue - (mValue >> 8) * 256); } }

        public JFIFVersion(ExifTag tag, ushort value)
            : base(tag, value)
        {
            ;
        }

        public JFIFVersion(ExifTag tag, byte major, byte minor)
            : base(tag, (ushort)(major * 256 + minor))
        {
            ;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1:00}", Major, Minor);
        }
    }
    /// <summary>
    /// Represents a JFIF thumbnail. (EXIF Specification: BYTE) 
    /// </summary>
    public class JFIFThumbnailProperty : ExifProperty
    {
        protected JFIFThumbnail mValue;
        protected override object _Value { get { return Value; } set { Value = (JFIFThumbnail)value; } }
        public new JFIFThumbnail Value { get { return mValue; } set { mValue = value; } }

        public override string ToString() { return mValue.Format.ToString(); }

        public JFIFThumbnailProperty(ExifTag tag, JFIFThumbnail value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                if (mValue.Format == JFIFThumbnail.ImageFormat.BMP24Bit)
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), InterOpType.BYTE, (uint)mValue.PixelData.Length, mValue.PixelData);
                else if (mValue.Format == JFIFThumbnail.ImageFormat.BMPPalette)
                {
                    byte[] data = new byte[mValue.Palette.Length + mValue.PixelData.Length];
                    Array.Copy(mValue.Palette, data, mValue.Palette.Length);
                    Array.Copy(mValue.PixelData, 0, data, mValue.Palette.Length, mValue.PixelData.Length);
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), InterOpType.BYTE, (uint)data.Length, data);
                }
                else if (mValue.Format == JFIFThumbnail.ImageFormat.JPEG)
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), InterOpType.BYTE, (uint)mValue.PixelData.Length, mValue.PixelData);
                else
                    throw new InvalidOperationException("Unknown thumbnail type.");
            }
        }
    }
}
