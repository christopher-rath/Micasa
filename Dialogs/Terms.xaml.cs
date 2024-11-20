#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2024 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using RichTextBoxExtensions;
using System.IO;
using System.Windows;

namespace Micasa
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Terms : Window
    {
        public Terms()
        {
            string licenseStr;
            string termsStrFlPath = @"Properties" + System.IO.Path.DirectorySeparatorChar + @"GNU_LGPL.rtf";

            InitializeComponent();

            try
            {
                licenseStr = File.ReadAllText(termsStrFlPath);
            }
            catch
            {
                licenseStr = @"Unexpected error reading About string from '" + termsStrFlPath + "'.";
            }

            licenseStr = licenseStr.Replace(Constants.sVersionToRepl, Constants.sMcVersion);
            licenseStr = licenseStr.Replace(Constants.sPlatformToRepl, Constants.sMcPlatform);
            rtbLicense.SetRtf(licenseStr.Replace(Constants.sCopyrightToRepl, Constants.sMcCopyright));
        }

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
