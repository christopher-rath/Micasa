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
    /// <summary>
    /// The exception that is thrown when the format of the JPEG/Exif file
    /// could not be understood.
    /// </summary>
    public class NotValidExifFileException : Exception
    {
        public NotValidExifFileException()
            : base("Not a valid JPEG/Exif file.")
        {
            ;
        }

        public NotValidExifFileException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the IFD section ID could not be understood.
    /// </summary>
    public class UnknownIFDSectionException : Exception
    {
        public UnknownIFDSectionException()
            : base("Unknown IFD section.")
        {
            ;
        }

        public UnknownIFDSectionException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when an invalid enum type is given to an 
    /// ExifEnumProperty.
    /// </summary>
    public class UnknownEnumTypeException : Exception
    {
        public UnknownEnumTypeException()
            : base("Unknown enum type.")
        {
            ;
        }

        public UnknownEnumTypeException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the 0th IFD section does not contain any fields.
    /// </summary>
    public class IFD0IsEmptyException : Exception
    {
        public IFD0IsEmptyException()
            : base("0th IFD section cannot be empty.")
        {
            ;
        }

        public IFD0IsEmptyException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when the format of the GIF file
    /// could not be understood.
    /// </summary>
    public class NotValidGIFFileException : Exception
    {
        public NotValidGIFFileException()
            : base("Not a valid GIF file.")
        {
            ;
        }

        public NotValidGIFFileException(string message)
            : base(message)
        {
            ;
        }
    }
}
