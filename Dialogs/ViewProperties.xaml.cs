#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2026 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using ExifLibrary;
using System.Collections.Generic;
using System.Windows;

namespace Micasa.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewProperties.xaml
    /// </summary>
    public partial class ViewProperties : Window
    {
        public ViewProperties()
        {
            InitializeComponent();
        }

        private class PropertyItem
        {
            public string Property { get; set; }
            public string Value { get; set; }
            public PropertyItem(string property, string value)
            {
                Property = property;
                Value = value;
            }
        }

        public void Show(string filename)
        {
            var file = ImageFile.FromFile(filename);
            var propList = new List<PropertyItem>();

            this.Title = this.Title + ": " + System.IO.Path.GetFileName(filename);
            this.tbFilename.Text = System.IO.Path.GetFileName(filename);
            foreach (var property in file.Properties)
            {
                propList.Add(new PropertyItem(property.Name, property.Value.ToString()));
            }
            dgProperties.ItemsSource = propList;

            base.Show();
        }

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
