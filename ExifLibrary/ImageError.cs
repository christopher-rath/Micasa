#region Copyright
// ExifLibrary - a .Net Standard library for editing Exif metadata contained in image files.
// Author: Özgür Özçıtak
// Based-on Version: 2.1.4
// Updates by: Christopher Rath <christopher@rath.ca>
// Archived at: https://oozcitak.github.io/exiflibrary/
// Copyright (c) 2013 Özgür Özçıtak
// Distributed under the MIT License (MIT) -- see http://opensource.org/licenses/MIT
// Warranty: None, see the license.
#endregion
namespace ExifLibrary
{
    /// <summary>
    /// Represents error severity.
    /// </summary>
    public enum Severity
    {
        Info,
        Warning,
        Error,
    }

    /// <summary>
    /// Represents errors or warnings generated while reading/writing image files.
    /// </summary>
    public class ImageError
    {

        /// <summary>
        /// Gets the severity of the error.
        /// </summary>
        public Severity Severity { get;}

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageError"/> class.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="message"></param>
        public ImageError(Severity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Message;
        }
    }
}
