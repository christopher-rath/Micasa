﻿#region Copyright
// ExifLibrary - a .Net Standard library for editing Exif metadata contained in image files.
// Author: Özgür Özçıtak
// Based-on Version: 2.1.4
// Updates by: Christopher Rath <christopher@rath.ca>
// Archived at: https://oozcitak.github.io/exiflibrary/
// Copyright (c) 2013 Özgür Özçıtak
// Distributed under the MIT License (MIT) -- see http://opensource.org/licenses/MIT
// Warranty: None, see the license.
#endregion
using System;

namespace ExifLibrary
{
#pragma warning disable CA1710 // Identifiers should have correct suffix
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
