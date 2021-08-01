#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021 © Christopher Rath
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

namespace Micasa
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class WatchListToIconConverter : IValueConverter
    {
        // See https://docs.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?view=netframeworkdesktop-4.8
        // for an explanation of the "pack:" Uri type.
        private static readonly BitmapImage blankBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Blank.png"));
        private static readonly BitmapImage excludeBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Exclude.png"));
        private static readonly BitmapImage oneTimeBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/OneTime.png"));
        private static readonly BitmapImage watchedBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Watched.png"));
        private static readonly Random rnd = new Random();
        // The single instance of WatchListToIconConverter.
        public static WatchListToIconConverter Instance = new WatchListToIconConverter();

        /// <summary>
        /// This WatchListToIconConverter class uses private constructor to implement the Singleton 
        /// design pattern.  The single instance of this class is referenced via
        /// WatchListToIconConverter.Instance.
        /// </summary>
        private WatchListToIconConverter() { }

        /// <summary>
        /// Take the path parameter and based upon whether the path occurs in either the
        /// one-time or monitor lists, return an icon to reflect the path's status.
        /// 
        /// TODO: Find a way to detect when the path we recieve is the one sent to us
        ///       so we can put an icom beside "This PC" at the top of the TreeView.
        ///       At the moment, because the XAML only passes us the ListViewItem's Tag
        ///       property there is no way to detect "This PC" (which is the string in
        ///       the Header).
        /// </summary>
        /// <param name="path">The string.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>A BitmapImage object--the icon to display.</returns>
        public object Convert(object path, Type targetType, object parameter, CultureInfo culture)
        {
            string pathStr = (path as string);

            if (pathStr.StartsWith(WatchedLists.ThisPCStr))
            {
                return blankBitmap;
            }
            else
            {
                switch (WatchedLists.FolderDisposition(pathStr))
                {
                    case WatchType.Watched:
                        return watchedBitmap;
                    case WatchType.Onetime:
                        return oneTimeBitmap;
                    case WatchType.Excluded:
                        return excludeBitmap;
                    default:
                        return blankBitmap;
                }
            }
        }

        public object ConvertBack(object path, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
