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
using System.IO;
using System.Windows;

namespace Micasa
{
    using RichtextboxExtensions;

    /// <summary>
    /// Fetch the contents of the About_Micasa.rtf file and display them in 
    /// About.xaml.
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            string aboutStr;
            var aboutStrFlPath = @"Properties" + Path.DirectorySeparatorChar + @"About_Micasa.rtf";

            InitializeComponent();

            try
            {
                aboutStr = File.ReadAllText(aboutStrFlPath);
            }
            catch (Exception e)
            {
                aboutStr = @"Unexpected error (" + e.Message + @") reading About string from Properties file,\par{\tab{" + aboutStrFlPath + @"}}";
            }

            // Note: URLs in the about text do not render in the About dialog as clickable links.
            // This is a known issue: https://github.com/dotnet/winforms/issues/3632
            aboutStr = aboutStr.Replace(Constants.sVersionToRepl, Constants.sMcVersion);
            aboutStr = aboutStr.Replace(Constants.sPlatformToRepl, Constants.sMcPlatform);
            rtbAboutText.SetRtf(aboutStr.Replace(Constants.sCopyrightToRepl, Constants.sMcCopyright));
        }

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
