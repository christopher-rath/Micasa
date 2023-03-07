using System;

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
    public static class ExifTagFactory
    {
        #region Static Methods
        /// <summary>
        /// Returns the ExifTag corresponding to the given tag id.
        /// </summary>
        public static ExifTag GetExifTag(IFD ifd, ushort tagid)
        {
            return (ExifTag)(ifd + tagid);
        }

        /// <summary>
        /// Returns the tag id corresponding to the given ExifTag.
        /// </summary>
        public static ushort GetTagID(ExifTag exiftag)
        {
            IFD ifd = GetTagIFD(exiftag);
            return (ushort)((int)exiftag - (int)ifd);
        }

        /// <summary>
        /// Returns the IFD section containing the given tag.
        /// </summary>
        public static IFD GetTagIFD(ExifTag tag)
        {
            return (IFD)(((int)tag / 100000) * 100000);
        }

        /// <summary>
        /// Returns the string representation for the given exif tag.
        /// </summary>
        public static string GetTagName(ExifTag tag)
        {
            string name = Enum.GetName(typeof(ExifTag), tag);
            if (name == null)
                return "Unknown";
            else
                return name;
        }

        /// <summary>
        /// Returns the string representation for the given tag id.
        /// </summary>
        public static string GetTagName(IFD ifd, ushort tagid)
        {
            return GetTagName(GetExifTag(ifd, tagid));
        }

        /// <summary>
        /// Returns the string representation for the given exif tag including 
        /// IFD section and tag id.
        /// </summary>
        public static string GetTagLongName(ExifTag tag)
        {
            string ifdname = Enum.GetName(typeof(IFD), GetTagIFD(tag));
            string name = Enum.GetName(typeof(ExifTag), tag);
            if (name == null)
                name = "Unknown";
            string tagidname = GetTagID(tag).ToString();
            return ifdname + ": " + name + " (" + tagidname + ")";
        }
        #endregion
    }
}
