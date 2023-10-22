#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2023 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using LiteDB;
using RichtextboxExtensions;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Micasa
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void OptionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Main Tab
            switch (Options.Instance.MyAppMode)
            {
                case AppMode.Legacy:
                    rbAppModeLegacy.IsChecked = true;
                    cbUpdPhotoFiles.IsChecked = Options.Instance.UpdatePhotoFiles;
                    // In Legacy mode, the UpdatePhotoFiles option is not used.
                    cbUpdPhotoFiles.IsEnabled = false;
                    break;
                case AppMode.Migrate:
                    rbAppModeMigrate.IsChecked = true;
                    cbUpdPhotoFiles.IsChecked = Options.Instance.UpdatePhotoFiles;
                    cbUpdPhotoFiles.IsEnabled = true;
                    break;
                case AppMode.Native:
                    rbAppModeNative.IsChecked = true;
                    cbUpdPhotoFiles.IsChecked = Options.Instance.UpdatePhotoFiles;
                    cbUpdPhotoFiles.IsEnabled = true;
                    break;
            }
            tbOpHomeFolderPath.Text = Options.iniFileNm;
            tbWatchedFoldersPath.Text = WatchedLists.watchedListFilename;
            tbExcludeFoldersPath.Text = WatchedLists.excludeListFilename;
            tbOneTimeFoldersPath.Text = WatchedLists.oneTimeListFilename;
            tbDatabasePath.Text = Database.DBFilename;
            // File Type Tab
            cbOpFileTypeAvi.IsChecked = Options.Instance.FileTypeAvi;
            cbOpFileTypeBmp.IsChecked = Options.Instance.FileTypeBmp;
            cbOpFileTypeGif.IsChecked = Options.Instance.FileTypeGif;
            cbOpFileTypeJpg.IsChecked = Options.Instance.FileTypeJpg;
            cbOpFileTypePng.IsChecked = Options.Instance.FileTypePng;
            cbOpFileTypeTga.IsChecked = Options.Instance.FileTypeTga;
            cbOpFileTypeTif.IsChecked = Options.Instance.FileTypeTif;
            cbOpFileTypeWebp.IsChecked = Options.Instance.FileTypeWebp;
            cbOpFileTypePsd.IsChecked = Options.Instance.FileTypePsd;
            cbOpFileTypeNef.IsChecked = Options.Instance.FileTypeNef;
            cbOpFileTypeMov.IsChecked = Options.Instance.FileTypeMov;
	    // Datgabase Tab
	    rtbRebuildInstr.SetRtf(Constants.sMcRebuildInstrRTF);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)rbAppModeLegacy.IsChecked)
            {
                Options.Instance.MyAppMode = AppMode.Legacy;
                Options.Instance.UpdatePhotoFiles = (bool)cbUpdPhotoFiles.IsChecked; ;
            }
            else if ((bool)rbAppModeMigrate.IsChecked)
            {
                Options.Instance.MyAppMode = AppMode.Migrate;
                Options.Instance.UpdatePhotoFiles = (bool)cbUpdPhotoFiles.IsChecked;
            }
            else if ((bool)rbAppModeNative.IsChecked)
            {
                Options.Instance.MyAppMode = AppMode.Native;
                Options.Instance.UpdatePhotoFiles = (bool)cbUpdPhotoFiles.IsChecked;
            }
            else
            {
                Options.Instance.MyAppMode = Options.Instance.DefaultAppMode;
                Options.Instance.UpdatePhotoFiles = (bool)cbUpdPhotoFiles.IsChecked;
            }
            Options.Instance.FileTypeAvi = (bool)cbOpFileTypeAvi.IsChecked;
            Options.Instance.FileTypeBmp = (bool)cbOpFileTypeBmp.IsChecked;
            Options.Instance.FileTypeGif = (bool)cbOpFileTypeGif.IsChecked;
            Options.Instance.FileTypeJpg = (bool)cbOpFileTypeJpg.IsChecked;
            Options.Instance.FileTypeMov = (bool)cbOpFileTypeMov.IsChecked;
            Options.Instance.FileTypePng = (bool)cbOpFileTypePng.IsChecked;
            Options.Instance.FileTypePsd = (bool)cbOpFileTypePsd.IsChecked;
            Options.Instance.FileTypeNef = (bool)cbOpFileTypeNef.IsChecked;
            Options.Instance.FileTypeTga = (bool)cbOpFileTypeTga.IsChecked;
            Options.Instance.FileTypeTif = (bool)cbOpFileTypeTif.IsChecked;
            Options.Instance.FileTypeWebp = (bool)cbOpFileTypeWebp.IsChecked;
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void Rebuild_Click(object sender, RoutedEventArgs e)
        {
            if (tbRebuildConfirm.Text == Constants.sMcRebuildConfirm)
            {
                tbRebuildConfirm.Text = "";

                using (LiteDatabase db = new(Database.ConnectionString(Database.DBFilename)))
                {
                    using (new WaitCursorIndicator(this))
                    {
                        // Stop the picture scanner before we rebuild the DB.
                        MainWindow.Stopscanners();
                        // Small DBs rebuild so quickly that adding a short pause provides a
                        // better user experience (i.e., it makes the busy cursor noticeably
                        // visible).  It also give the scanners time to stop.
                        Thread.Sleep(2000);
                        db.Rebuild();
                        // Restart the scanners now that the rebuild is done.
                        MainWindow.StartScanners();
                    }
                }
            }
	    }
	
        private void RbAppModeLegacy_Checked(object sender, RoutedEventArgs e)
        {
            cbUpdPhotoFiles.IsEnabled = false;
            cbUpdPhotoFiles.Foreground = Brushes.Gray;
        }

        private void RbAppModeMigrate_Checked(object sender, RoutedEventArgs e)
        {
            cbUpdPhotoFiles.IsEnabled = true;
            cbUpdPhotoFiles.Foreground = Brushes.Black;
        }

        private void RbAppModeNative_Checked(object sender, RoutedEventArgs e)
        {
            cbUpdPhotoFiles.IsEnabled = true;
            cbUpdPhotoFiles.Foreground = Brushes.Black;
        }
    }
}
