﻿#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca> 
// Archived at: http://rath.ca/
// Copyright 2021-2025 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using RichTextBoxExtensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Micasa
{
    public partial class About : Window
    {
        /// <summary>
        /// Fetch the contents of the About_Micasa.rtf file and display them in 
        /// About.xaml.
        /// </summary>
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

            // Note: URLs in the about text do not automatically render in the About dialog as clickable links.
            // This is a known issue: https://github.com/dotnet/winforms/issues/3632
            //
            // 2023-12-24 (v0.7): I've implemented code to manually detect URLs as the panel is loaded.
            aboutStr = aboutStr.Replace(Constants.sVersionToRepl, Constants.sMcVersion);
            aboutStr = aboutStr.Replace(Constants.sPlatformToRepl, Constants.sMcPlatform);
            rtbAboutText.SetRtf(aboutStr.Replace(Constants.sCopyrightToRepl, Constants.sMcCopyright));
            rtbAboutText.MakeUrlsClickable();
            rtbAboutText.EmailToMailto(Constants.sAuthorEmail);
        }

        /// <summary>
        /// Codebehind in support of the &lt;Setter&gt; contained in the RishTextBox control.  This codebehind
        /// and the &lt;Setter&gt; are needed to support the MakeUrlsClickable() extension to the WPF RichTextBox
        /// control.  See "ExtensionsForRTB.cs" for more info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            Process.Start(new ProcessStartInfo(hyperlink.NavigateUri.ToString())
            {
                UseShellExecute = true,
            });
            e.Handled = true;
        }

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
