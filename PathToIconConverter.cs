#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2023 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using StringExtensions;

namespace Micasa
{
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
        public static PathToIconConverter Instance = new();

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
        public object Convert(object path, Type targetType, object parameter, CultureInfo culture)
        {
            string pathStr = (path as string);

            pathStr = pathStr.RmPrefix(WatchedLists.ThisPCStr);
            if (pathStr.EndsWith(@":\"))
            {
                return driveBitmap;
            }
            else if (pathStr.Equals(WatchedLists.ThisPCStr) || pathStr.Equals(WatchedLists.ComputerPath))
            {
                return computerBitmap;
            }
            else if (pathStr.Equals(WatchedLists.DesktopPath))
            {
                return desktopBitmap;
            } else if (pathStr.Equals(WatchedLists.DocumentsPath))
            {
                return documentsBitmap;
            } else if (pathStr.Equals(WatchedLists.PicturesPath))
            {
                return picturesBitmap;
            } else
            {
                return folderBitmap;
            }
        }

        public object ConvertBack(object path, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
