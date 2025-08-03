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
using System.Windows;
using System.Windows.Data;

namespace Micasa
{
    /// <summary>
    /// This IValueConverter is called by the UI to determine the visibility of 
    /// an item in the menubar.  The XAML code is configured to call this method
    /// to determine whether to show or hide the item based on the value returned
    /// by the items CanExecute method.  Where the CanExecute method returns
    /// false, the menu item is hidden.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToCollapsedVisibilityConverter : IValueConverter
    {
        // The single instance of BooleanToCollapsedVisibilityConverter.
        public static BooleanToCollapsedVisibilityConverter Instance = new();

        /// <summary>
        /// This BooleanToCollapsedVisibilityConverter class uses private constructor to implement  
        /// the Singletondesign pattern.  The single instance of this class is referenced via
        /// BooleanToCollapsedVisibilityConverter.Instance.
        /// </summary>
        private BooleanToCollapsedVisibilityConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //reverse conversion (false=>Visible, true=>collapsed) on any given parameter
            bool input = (null == parameter) ? (bool)value : !(bool)value;
            return input ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotSupportedException("Cannot convert back");
    }
}
