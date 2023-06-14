﻿#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2023 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using RichtextboxExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Micasa
{
    /// <summary>
    /// Fetch the contents of the Release_Notes.rtf file and display them in 
    /// ReleaseNotes.xaml.
    /// </summary>
    public partial class ReleaseNotes : Window
    {
        public ReleaseNotes()
        {
            string releaseNotesStr;
            string releaseNotesStrFlPath = @"Properties" + System.IO.Path.DirectorySeparatorChar + @"Release_Notes.rtf";

            InitializeComponent();
            try
            {
                releaseNotesStr = File.ReadAllText(releaseNotesStrFlPath);
            }
            catch (Exception e)
            {
                releaseNotesStr = @"Unexpected error (" + e.Message + @") reading Release Notes string from Properties file,\par{\tab{" + releaseNotesStrFlPath + @"}}";
            }
            rtbAboutText.SetRtf(releaseNotesStr);
	}

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
