#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2024 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Windows;
using System.Windows.Data;

namespace Micasa
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToCollapsedVisibilityConverter : IValueConverter
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        // The single instance of BooleanToCollapsedVisibilityConverter.
        public static BooleanToCollapsedVisibilityConverter Instance = new();
#pragma warning restore CA2211 // Non-constant fields should not be visible

        /// <summary>
        /// This BooleanToCollapsedVisibilityConverter class uses private constructor to implement  
        /// the Singletondesign pattern.  The single instance of this class is referenced via
        /// BooleanToCollapsedVisibilityConverter.Instance.
        /// </summary>
        private BooleanToCollapsedVisibilityConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //reverse conversion (false=>Visible, true=>collapsed) on any given parameter
            bool input = (null == parameter) ? (bool)value : !((bool)value);
            return (input) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
