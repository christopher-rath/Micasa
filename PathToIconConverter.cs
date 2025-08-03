#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2025 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using StringExtensions;

namespace Micasa
{
    /// <summary>
    /// This IValueConverter is called by the TreeView lists in both the FolderManager dialog
    /// and the MainWindow FolderTab to determine what icon to display beside each folder name.
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class PathToIconConverter : IValueConverter
    {
        // See https://docs.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?view=netframeworkdesktop-4.8
        // for an explanation of the "pack:" Uri type.
        private static readonly BitmapImage computerBitmap = new(new Uri("pack://application:,,,/Resources/Computer.png"));
        private static readonly BitmapImage driveBitmap = new(new Uri("pack://application:,,,/Resources/HardDrive.png"));
        private static readonly BitmapImage desktopBitmap = new(new Uri("pack://application:,,,/Resources/Desktop.png"));
        private static readonly BitmapImage documentsBitmap = new(new Uri("pack://application:,,,/Resources/DocumentsFolder.png"));
        private static readonly BitmapImage folderBitmap = new(new Uri("pack://application:,,,/Resources/FolderClosed.png"));
        private static readonly BitmapImage picturesBitmap = new(new Uri("pack://application:,,,/Resources/ImageGroup.png"));
        // The single instance of PathToIconConverter.
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static PathToIconConverter Instance = new();
#pragma warning restore CA2211 // Non-constant fields should not be visible

        /// <summary>
        /// This PathToIconConverter class uses private constructor to implement the Singleton 
        /// design pattern.  The single instance of this class is referenced via
        /// PathToIconConverter.Instance.
        /// </summary>
        private PathToIconConverter() { }

        /// <summary>
        /// Examine the path parameter and choose the icon that represents that path.
        /// </summary>
        /// <param name="path">The string.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>A BitmapImage object--the icon to display.</returns>
#pragma warning disable CA1725 // Parameter names should match base declaration
        public object Convert(object path, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore CA1725 // Parameter names should match base declaration
        {
            string pathStr = (path as string) ??
                throw new ArgumentNullException(nameof(path), "PathToIconConverter: path cannot be null");

            pathStr = pathStr.RmPrefix(WatchedLists.ThisPCStr);
            if (pathStr.EndsWith(@":\", StringComparison.Ordinal) || pathStr.EndsWith(':'))
            {
                return driveBitmap;
            }
            else if (pathStr.Equals(WatchedLists.ThisPCStr, StringComparison.Ordinal) || pathStr.Equals(WatchedLists.ComputerPath, StringComparison.Ordinal))
            {
                return computerBitmap;
            }
            else if (pathStr.Equals(WatchedLists.DesktopPath, StringComparison.Ordinal))
            {
                return desktopBitmap;
            } else if (pathStr.Equals(WatchedLists.DocumentsPath, StringComparison.Ordinal))
            {
                return documentsBitmap;
            } else if (pathStr.Equals(WatchedLists.PicturesPath, StringComparison.Ordinal))
            {
                return picturesBitmap;
            } else
            {
                return folderBitmap;
            }
        }

#pragma warning disable CA1725 // Parameter names should match base declaration
        public object ConvertBack(object path, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore CA1725 // Parameter names should match base declaration
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
