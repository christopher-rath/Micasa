#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2026 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Micasa
{
    /// <summary>
    /// This IValueConverter is called to load an image into Micasa.  This converter
    /// is used instead of the standard BitmapImage Uri constructor so that I can
    /// ensure that the file is closed once the image is loaded.  Although Microsoft's
    /// documentation for BitmapImage says that the file is closed after loading when
    /// the CacheOption is set to OnLoad, my personal testing with Micasa found that
    /// Micasa wasn't closing files after loading them.
    ///
    /// The idea to load photos through a converter to put Micasa in control of closing
    /// file handles came from this blog post by Rick Strahl:
    /// https://weblog.west-wind.com/posts/2025/Apr/28/WPF-Image-Control-Local-File-Locking
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class LocalFileImageConverter : IValueConverter
    {
        // The single instance of LocalFileImageConverter.
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static LocalFileImageConverter Instance = new();
#pragma warning restore CA2211 // Non-constant fields should not be visible

        /// <summary>
        /// This LocalFileImageConverter class uses private constructor to implement the  
        /// Singleton design pattern.  The single instance of this class is referenced via
        /// LocalFileImageConverter.Instance.
        /// </summary>
        private LocalFileImageConverter() { }

        /// <summary>
        /// Load the photo from the file path and then close the file handle to release
        /// the file for other processe to access.
        /// </summary>
        /// <param name="path">Full fully qualified filename of the photo to load.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>A BitmapImage containing the photo.</returns>
        /// <exception cref="ArgumentNullException"></exception>
#pragma warning disable CA1725 // Parameter names should match base declaration
        public object Convert(object path, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore CA1725 // Parameter names should match base declaration
        {
            string pathStr = (path as string) ??
                throw new ArgumentNullException(nameof(path), "LocalFileImageConverter: path cannot be null");
            BitmapImage bi = new BitmapImage();

            try
            {
                using (var fstream = new FileStream(pathStr, FileMode.Open, FileAccess.Read,
                    FileShare.Read))
                {
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = fstream;
                    bi.StreamSource.Flush();
                    bi.EndInit();
                    bi.Freeze();

                    bi.StreamSource.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LocalFileImageConverter: Error loading image: {ex.Message}");
            }

            return bi;
        }

#pragma warning disable CA1725 // Parameter names should match base declaration
        public object ConvertBack(object path, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore CA1725 // Parameter names should match base declaration
        {
            throw new NotImplementedException("LocalFileImageConverter: Two way conversion is not supported.");
        }
    }
}
