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
    /// Interaction logic for ReleaseNotes.xaml
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
