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
    /// The exception that is thrown when the format of the image file
    /// could not be understood.
    /// </summary>
    public class UnknownImageFormatException : Exception
    {
        public UnknownImageFormatException()
            : base("Unkown image format.")
        {
            ;
        }

        public UnknownImageFormatException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the format of the image file
    /// could not be understood.
    /// </summary>
    public class NotValidImageFileException : Exception
    {
        public NotValidImageFileException()
            : base("Not a valid image file.")
        {
            ;
        }

        public NotValidImageFileException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the format of the JPEG file
    /// could not be understood.
    /// </summary>
    public class NotValidJPEGFileException : Exception
    {
        public NotValidJPEGFileException()
            : base("Not a valid JPEG file.")
        {
            ;
        }

        public NotValidJPEGFileException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the format of the TIFF file
    /// could not be understood.
    /// </summary>
    public class NotValidTIFFileException : Exception
    {
        public NotValidTIFFileException()
            : base("Not a valid TIFF file.")
        {
            ;
        }

        public NotValidTIFFileException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the format of the PNG file
    /// could not be understood.
    /// </summary>
    public class NotValidPNGFileException : Exception
    {
        public NotValidPNGFileException()
            : base("Not a valid PNG file.")
        {
            ;
        }

        public NotValidPNGFileException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the format of the TIFF header
    /// could not be understood.
    /// </summary>
    public class NotValidTIFFHeader : Exception
    {
        public NotValidTIFFHeader()
            : base("Not a valid TIFF header.")
        {
            ;
        }

        public NotValidTIFFHeader(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the length of a section exceeds 64 kB.
    /// </summary>
    public class SectionExceeds64KBException : Exception
    {
        public SectionExceeds64KBException()
            : base("Section length exceeds 64 kB.")
        {
            ;
        }

        public SectionExceeds64KBException(string message)
            : base(message)
        {
            ;
        }
    }
}
