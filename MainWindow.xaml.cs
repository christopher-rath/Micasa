#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2026 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using LiteDB;
using Micasa.Dialogs;
using StringExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace Micasa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private static CancellationTokenSource DeletedScannerCancellationSource = new();
        private static CancellationToken DeletedScannerCancellationToken = DeletedScannerCancellationSource.Token;
        private static CancellationTokenSource PictureScannerCancellationSource = new();
        private static CancellationToken PictureScannerCancellationToken = PictureScannerCancellationSource.Token;
        private static readonly PictureWatcher _ActiveWatchers = new();
        private static CancellationTokenSource PictureProcessorCancellationSource = new();
        private static CancellationToken PictureProcessorCancellationToken = PictureProcessorCancellationSource.Token;
        private static CancellationTokenSource FolderTabCancellationSource = new();
        private static CancellationToken FolderTabCancellationToken = FolderTabCancellationSource.Token;
        private TreeViewItem SelectedItem = null;
        private ILiteCollection<PhotosTbl> PhotoCol = null;
        private ILiteCollection<FoldersTbl> FolderCol = null;
        // use with zoom // private static readonly Regex rgxNumOnly = new Regex("[0-9]+");

        #region MenuRoutedCommands
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static RoutedCommand AboutCmd = new();
        public static RoutedCommand AddFileCmd = new();
        public static RoutedCommand AddFolderCmd = new();
        public static RoutedCommand AddToScrnSvrCmd = new();
        public static RoutedCommand AdjustDateCmd = new();
        public static RoutedCommand AutomaticCmd = new();
        public static RoutedCommand BackupCmd = new();
        public static RoutedCommand BatchUploadCmd = new();
        public static RoutedCommand ConfigButtonsCmd = new();
        public static RoutedCommand ConfigScrnSavCmd = new();
        public static RoutedCommand ConfigViewCmd = new();
        public static RoutedCommand CreateGCDCmd = new();
        public static RoutedCommand DeleteCmd = new();
        public static RoutedCommand EditDescCmd = new();
        public static RoutedCommand EditViewCmd = new();
        public static RoutedCommand EmailCmd = new();
        public static RoutedCommand ExitCmd = new();
        public static RoutedCommand ExportCmd = new();
        public static RoutedCommand ExportHTMLCmd = new();
        public static RoutedCommand ExportToDVRCmd = new();
        public static RoutedCommand FolderMgrCmd = new();
        public static RoutedCommand ForumsCmd = new();
        public static RoutedCommand HelpContentsCmd = new();
        public static RoutedCommand HiddenPictCmd = new();
        public static RoutedCommand HideCmd = new();
        public static RoutedCommand ImportFromCmd = new();
        public static RoutedCommand LibraryViewCmd = new();
        public static RoutedCommand LocateFolderOnDiskCmd = new();
        public static RoutedCommand LocateOnDiskCmd = new();
        public static RoutedCommand MakeAPosterCmd = new();
        public static RoutedCommand MoveCmd = new();
        public static RoutedCommand MoveFolderCmd = new();
        public static RoutedCommand NewAlbumCmd = new();
        public static RoutedCommand NrmlThumbCmd = new();
        public static RoutedCommand OpenFileInEditorCmd = new();
        public static RoutedCommand OptionsCmd = new();
        public static RoutedCommand PeopleCmd = new();
        public static RoutedCommand PeopleMgrCmd = new();
        public static RoutedCommand PictureCollageCmd = new();
        public static RoutedCommand PlacesCmd = new();
        public static RoutedCommand PrintContactCmd = new();
        public static RoutedCommand PrivacyCmd = new();
        public static RoutedCommand PropertiesCmd = new();
        public static RoutedCommand PublishToBlgrCmd = new();
        public static RoutedCommand ReadmeCmd = new();
        public static RoutedCommand RefreshThumbsCmd = new();
        public static RoutedCommand ReleaseNotesCmd = new();
        public static RoutedCommand RemoveFromCmd = new();
        public static RoutedCommand RenameCmd = new();
        public static RoutedCommand ResetFacesCmd = new();
        public static RoutedCommand RevertCmd = new();
        public static RoutedCommand SaveACopyCmd = new();
        public static RoutedCommand SearchOptCmd = new();
        public static RoutedCommand SetAsDesktopCmd = new();
        public static RoutedCommand ShortcutsCmd = new();
        public static RoutedCommand ShowEditCtrlsCmd = new();
        public static RoutedCommand SlideshowCmd = new();
        public static RoutedCommand SmallPictCmd = new();
        public static RoutedCommand SmlThumbCmd = new();
        public static RoutedCommand TagsCmd = new();
        public static RoutedCommand TermsCmd = new();
        public static RoutedCommand TimelineCmd = new();
        public static RoutedCommand UndoAddEditsCmd = new();
        public static RoutedCommand UnhideCmd = new();
        public static RoutedCommand UninstallingCmd = new();
        public static RoutedCommand UpdatesCmd = new();
        public static RoutedCommand UploadMgrCmd = new();
        public static RoutedCommand UseClrMgmtCmd = new();
        public static RoutedCommand ViewAndEditCmd = new();
        public static RoutedCommand ViewSlidesCmd = new();
#pragma warning restore CA2211 // Non-constant fields should not be visible
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Custom code.
            Instance = this;
            // Load a default message in the StatusBar.  What we load depends whether or not the 
            // user has configured any folders to watch.
            if (WatchedLists.Instance.IsWatchingFolders)
            {
                this.tbStatusMsg.Text = Constants.sMcAppName + " is watching folders; open the Folder Manager to see the list.";
            }
            else
            {
                this.tbStatusMsg.Text = "Open the Folder Manager (Tools→Folder Manager) and configure folders to be watched by Micasa.";
            }

            // Ensure that the Micasa data folder exists (where its database and other data is stored).
            try
            {
                Directory.CreateDirectory(AppData + Path.DirectorySeparatorChar + Constants.sMcAppDataFolder);
            }
            catch
            {
                string msg = "ERROR: unable to create Micasa roaming folder (" + AppData + Path.DirectorySeparatorChar
                            + Constants.sMcAppDataFolder + ").\n\nUnable to continue.";
                MessageBox.Show(msg, "Micasa: Roaming Folder Creation Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }

            // Open (or create if it doesn't exist) the Micasa database.
            try
            {
                Database.CreateDB();
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.Message, "Micasa: Unexpected ArgumentException Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }
            catch (IOException e)
            {
                string msg = @"Micasa: Unexpected IOException error opening the Micasa database (" + e.Message + @").";
                MessageBox.Show(msg, "Micasa: Database Open Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }

            // Start the new/updated/deleted picture scanner.
            try
            {
                StartScanners();
            }
            catch (Exception e)
            {
                string msg = $"ERROR: unexpected error starting scanners: {e.Message}\n\nUnable to continue.";
                MessageBox.Show(msg, "Micasa: StartScanners Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }

            // Start the FileSystemWatchers (watch for changes to watched folders).
            try
            {
                StartWatchers();
            }
            catch (Exception e)
            {
                string msg = $"ERROR: unexpected error setting up FileSystemWatchers for watched folders: {e.Message}\n\nUnable to continue.";
                MessageBox.Show(msg, "Micasa: FileSystemWatcher Creation Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }

            // If we got to this point in the constructor then a database exists; so, we'll
            // open it and get the PhotoCol collection for use later.
            var db = new LiteDatabase(Database.ConnectionString(Database.DBFilename));
            Instance.PhotoCol = db.GetCollection<PhotosTbl>(Constants.sMcPhotosColNm);
        }

        #region GetterSetters
        public static string AppData { get; } = Environment.ExpandEnvironmentVariables(@"%APPDATA%");

        /// <summary>
        /// Return the name of the tab that is active in the left-hand side of the MainWindow.
        /// </summary>
        public static string ActiveLeftTab
        {
            get
            {
                if (Instance.MWFolderTab.IsSelected)
                {
                    return Instance.MWFolderTab.Name;
                }
                else if (Instance.MWAlbumsTab.IsSelected)
                {
                    return Instance.MWAlbumsTab.Name;
                }
                else if (Instance.MWLtPeopleTab.IsSelected)
                {
                    return Instance.MWLtPeopleTab.Name;
                }
                else
                {
                    // Default to the Folders tab.
                    return Instance.MWFolderTab.Name;
                }
            }
        }

        /// <summary>
        /// Return the name of the tab that is active in the left-hand side of the MainWindow.
        /// </summary>
        public static string ActiveRightTab
        {
            get
            {
                if (Instance.MWRtPeopleTab.IsSelected)
                {
                    return Instance.MWRtPeopleTab.Name;
                }
                else if (Instance.MWDetailsTab.IsSelected)
                {
                    return Instance.MWDetailsTab.Name;
                }
                else if (Instance.MWMapTab.IsSelected)
                {
                    return Instance.MWMapTab.Name;
                }
                else
                {
                    // Default to the Details tab.
                    return Instance.MWDetailsTab.Name;
                }
            }
        }
        #endregion GetterSetters

        #region Event_Handlers
        /// <summary>
        /// Once the MainWindow has been loaded, populate the three tabs (Folders, Albums,
        /// and People); start a separate thread for populating each tab.
        /// 
        /// To Do: retrieve the tab and tab-item that was active when Micasa was last active,
        /// and make that tab and tab-item active for this session.  This likely cannot be
        /// done until after the three tabs have been completely populated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FolderTabCancellationSource = new CancellationTokenSource();
            FolderTabCancellationToken = FolderTabCancellationSource.Token;
            Task.Run(() => StartFolderTab(FolderTabCancellationToken), FolderTabCancellationToken);
        }

        /// <summary>
        /// The actions to take as the application shuts down:
        ///  * Stop the scanners.
        ///  * Stop the watchers.
        ///  
        /// To Do: save the active tab and tab-item so that it can be restored when the
        /// application next starts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stopscanners();
            _ActiveWatchers.StopWatchers();
            StopNavigationTabs();
        }
        #endregion Event_Handlers

        #region Thread_Code

        /// <summary>
        /// Start the new/updated/deleted picture scanners.  This is done using
        /// two separate scanners: one for new/updated pictures and one for deleted
        /// pictures.  The two scanners are started in separate threads.
        /// </summary>
        public static void StartScanners()
        {
            // We always create a fresh token in case the existing one is in a cancalled state.
            PictureScannerCancellationSource = new CancellationTokenSource();
            PictureScannerCancellationToken = PictureScannerCancellationSource.Token;
            Task.Run(() => PictureScanner.StartScanner(PictureScannerCancellationToken), PictureScannerCancellationToken);

            // We always create a fresh token in case the existing one is in a cancalled state.
            DeletedScannerCancellationSource = new CancellationTokenSource();
            DeletedScannerCancellationToken = DeletedScannerCancellationSource.Token;
            Task.Run(() => DeletedScanner.StartScanner(DeletedScannerCancellationToken), DeletedScannerCancellationToken);
        }

        /// <summary>
        /// Stop the two scanners.
        /// </summary>
        public static void Stopscanners()
        {
            PictureScannerCancellationSource.Cancel();
            DeletedScannerCancellationSource.Cancel();
        }

        /// <summary>
        /// Start the watchers; one watcher for each folder in the WatchedFolders list.
        /// Then start the associated changed-folder processor in its own thread.
        /// </summary>
        public static void StartWatchers()
        {
            try
            {
                // First, star the FileSystemWatcher daemons.
                foreach (string wPath in WatchedLists.Instance.WatchedFolders)
                {
                    // Ignore folders that don't exist.
                    if (Directory.Exists(wPath))
                    {
                        _ActiveWatchers.WatchFolder(wPath);
                    }
                }
                // Then, start the thread that processes the photo Queue<T>.
                // We always create a fresh token in case the existing one is in a cancalled state.
                PictureProcessorCancellationSource = new CancellationTokenSource();
                PictureProcessorCancellationToken = PictureProcessorCancellationSource.Token;
                Task.Run(() => PictureWatcher.StartProcessor(PictureProcessorCancellationToken), PictureProcessorCancellationToken);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// The Public method to call to stop the watchers and the associated processor.
        /// </summary>
        public static void StopWatchers()
        {
            _ActiveWatchers.StopWatchers();
            PictureProcessorCancellationSource.Cancel();
        }
        #endregion Thread_Code

        #region MainWindowFolderList
        public static void StartFolderTab(object token)
        {
            CancellationToken myCancelToken = (CancellationToken)token;

            // Populate the Folders tab with the folders that contain photos that
            // Micasa has discovered; that is, for each Pathname in the FoldersCol
            // database table, create a TreeViewItem.
            //Debug.WriteLine("StartFolderTab: populating TreeView.");
            // Open the database.
            using (var db = new LiteDatabase(Database.ConnectionString(Database.DBFilename)))
            {
                Instance.FolderCol = db.GetCollection<FoldersTbl>(Constants.sMcFoldersColNm);

                // Iterate through the Pathname entries in FolderCol database table, ascending alphanumeric order.
                var query = Instance.FolderCol.Query()
                    .OrderBy(x => x.Pathname)
                    .ToEnumerable();

                foreach (var folderRow in query.ToList())
                {
                    if (myCancelToken.IsCancellationRequested)
                    {
                        // Stop populating the tab if the cancellation token has been set.
                        break;
                    }
                    else
                    {
                        Debug.WriteLine("StartFolderTab: adding to TreeView: " + folderRow.Pathname);

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            AddPathToTree(Instance.dbFoldersItem, folderRow.Pathname);
                        });
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ClearImageDetails();
                    SelectATab(Instance.NavigationTabs, Options.Instance.LastSelectedLeftTab);
                    SelectATab(Instance.InfoTabs, Options.Instance.LastSelectedRightTab);
                    SelectAFolder(Instance.dbFoldersItem, Options.Instance.LastSelectedFolder);
                });
            }
        }

        /// <summary>
        /// The Public method to call to stop the threads that populate the MainWindow tabs.
        /// </summary>
        public static void StopNavigationTabs()
        {
            FolderTabCancellationSource.Cancel();
        }

        private void DbFoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem item = (TreeViewItem)tree.SelectedItem;
            string path = (string)item.Tag;

            SelectedItem = item;
        }

        private void NavigationTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MWFolderTab.IsSelected)
            {
                // Load a folder icon into the imgHdrIcon Image widget.
                imgHdrIcon.CacheMode = new BitmapCache();
                imgHdrIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/FolderClosed.png"));
                // Post the selected folder's name to the header.
                Instance.tbPhotosHeader.Text = Path.GetFileName(Options.Instance.LastSelectedFolder.TrimEnd(Path.DirectorySeparatorChar,
                                                                            Path.AltDirectorySeparatorChar));
            }
            else if (MWAlbumsTab.IsSelected)
            {
                // Load an album icon into the imgHdrIcon Image widget.
                imgHdrIcon.CacheMode = new BitmapCache();
                imgHdrIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Album.png"));
                // Clear the name in the header (since the Albums functionality isn't yet coded.
                Instance.tbPhotosHeader.Text = string.Empty;
            }
            else if (MWLtPeopleTab.IsSelected)
            {
                // Load a people icon into the imgHdrIcon Image widget.
                imgHdrIcon.CacheMode = new BitmapCache();
                imgHdrIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/People.png"));
                // Clear the name in the header (since the People functionality isn't yet coded.
                Instance.tbPhotosHeader.Text = string.Empty;
            }
        }

        private void DbFoldersItem_Selected(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            // Walk back up the tree to assemble the full path of the selected folder.
            if (e.OriginalSource is TreeViewItem item)
            {
                var stack = new Stack<string>();
                TreeViewItem current = item;
                while (current != null)
                {
                    if (current.Tag is string folder)
                    {
                        stack.Push(folder);
                    }
                    current = ItemsControl.ItemsControlFromItemContainer(current) as TreeViewItem;
                }
                string path = string.Join(Path.DirectorySeparatorChar, stack);

                //MessageBox.Show($"Item clicked: {path}");
                Debug.WriteLine($"DbFoldersItem_Selected: Item clicked: {path}.");
                Debug.WriteLine("DbFoldersItem_Selected: ...retrieving photos.");
                // Save the last selected folder.  Note that the Path.GetFileName() method is poorly
                // named because what it actually gets is that final component of the path; whether
                // it's an actual filename or just the final folder in the path.
                Options.Instance.LastSelectedFolder = path;
                // Get the final folder name from the path and post it to the tbPhotosHeader textblock.
                Instance.tbPhotosHeader.Text = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar,
                                                                            Path.AltDirectorySeparatorChar));
                // Open the database.
                using (var db = new LiteDatabase(Database.ConnectionString(Database.DBFilename)))
                {
                    ILiteCollection<PhotosTbl> LclPhotoCol = db.GetCollection<PhotosTbl>(Constants.sMcPhotosColNm);

                    // Iterate through the entries in PhotoCol database table, where the Pathname equals
                    // the TreeView path, sorted in ascending alphanumeric order.
                    var query = LclPhotoCol.Query()
                        .Where(x => x.Pathname.Equals(path, StringComparison.Ordinal))
                        .OrderBy(x => x.Picture)
                        .ToEnumerable();

                    // Clear the lbMainWindowPhotos Listbox.
                    lbMainWindowPhotos.Items.Clear();

                    foreach (var photoRow in query.ToList())
                    {
                        // Add the photo to the lbMainWindowPhotos Listbox as a ListboxItem.
                        Debug.WriteLine($"DbFoldersItem_Selected: adding photo to PhotoList: {photoRow.Picture}");

                        // The Uri() method is used to transform the Windows fully qualified filename
                        // into a format that can be used by the ListBox ListItem.
                        Uri uri = new Uri(photoRow.FQFilename);
                        // In the XAML for the Image widget contained in the ListBoxItem template:
                        //  * we have included a BitmapCache for the Image's CacheMode property; and,
                        //  * we have set the BitmapImage's CacheOption to OnLoad.
                        lbMainWindowPhotos.Items.Add(new BitmapImage(uri));
                    }
                }
            }
            else
            {
                Debug.WriteLine("DbFoldersItem_Selected: e.OriginalSource is not a TreeViewItem.");
                MessageBox.Show("DbFoldersItem_Selected: e.OriginalSource is not a TreeViewItem.");
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        }

        /// <summary>
        /// This method is invoked when the user clicks on a photo in the lbMainWindowPhotos Listbox.
        /// It retrieves the file details and metadata for the selected photo and populates the
        /// fields in the Details tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbMainWindowPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected image object
            if (lbMainWindowPhotos.SelectedItem != null)
            {
                var filename = lbMainWindowPhotos.SelectedItem.ToString().RmPrefix("file:///");

                // The ListItem string returns the Uri, which has slashes instead of
                // backslashes as the path separator; so, before we can use the ListItem's
                // string we have to swap the path separator back into Windows' form.
                filename = filename.Replace('/', Path.DirectorySeparatorChar);
                LoadImageDetails(filename);
            }
        }

        /// <summary>
        /// This method is invoked when the user clicks on a photo in the lbMainWindowPhotos Listbox.
        /// It retrieves the file details and metadata for the selected photo and populates the
        /// fields in the Details tab.
        /// </summary>
        /// <param name="filename">The fully qualified filename of the selected image file.</param>
        static private void LoadImageDetails(string filename)
        {
            Debug.WriteLine($"LoadImageDetails: Image selected: {filename}");
            // Retrieve the PhotoCol database record for the selected photo using the
            // fully qualified filename.
            try
            {
                var query = Instance.PhotoCol.Query()
                    .Where(x => x.FQFilename.Equals(filename, StringComparison.Ordinal));

                if (query == null || query.Count() == 0)
                {
                    Debug.WriteLine($"LoadImageDetails: No database record found for selected photo: {filename}");
                    MessageBox.Show($"No database record found for selected photo: {filename}");
                }
                else if (query.Count() > 1)
                {
                    Debug.WriteLine($"LoadImageDetails: Multiple database records found for selected photo: {filename}");
                    MessageBox.Show($"Multiple database records found for selected photo: {filename}");
                }
                else
                {
                    try
                    {
                        //Metadata metadata = new Metadata(filename);
                        var basename = Path.GetFileName(filename);
                        //FileInfo fileInfo = new FileInfo(filename);
#pragma warning disable CA1416 // Validate platform compatibility
                        //FileSecurity fileSecurity = fileInfo.GetAccessControl();
                        //IdentityReference owner = fileSecurity.GetOwner(typeof(NTAccount));
#pragma warning restore CA1416 // Validate platform compatibility

                        ClearImageDetails();
                        // Populate File Details fields.
                        Instance.tbFilename.Text = basename;
                        Instance.tbLocation.Text = Path.GetDirectoryName(filename);
                        Instance.tbFileSize.Text = $"{(query.FirstOrDefault().FileSize / (1024.0 * 1024.0)).ToString("N2", CultureInfo.InvariantCulture)} MB";
                        Instance.tbCreatedDate.Text = query.FirstOrDefault().CreatedDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        Instance.tbModifiedDate.Text = query.FirstOrDefault().ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        Instance.tbFileOwner.Text = query.FirstOrDefault().FileOwner;

                        // Populate Image Metadata fields.
                        Instance.tbTitleCaption.Text = query.FirstOrDefault().TitleCaption;

                        // Populate EXIF Metadata fields.
                        Instance.tbDimensions.Text = Metadata.FormatDimensions(query.FirstOrDefault().XDimension,
                                                        query.FirstOrDefault().YDimension, " pixels");
                        Instance.tbCameraMake.Text = query.FirstOrDefault().CameraMake;
                        Instance.tbCameraModel.Text = query.FirstOrDefault().CameraModel;
                        Instance.tbImgCreationDate.Text = query.FirstOrDefault().ImgCreationDate;
                        Instance.tbImgDigitisedDate.Text = query.FirstOrDefault().ImgDigitisedDate;
                        Instance.tbOrientation.Text = query.FirstOrDefault().Orientation;
                        Instance.tbFlash.Text = query.FirstOrDefault().Flash;
                        Instance.tbLens.Text = query.FirstOrDefault().LensMaker + " "
                                                + query.FirstOrDefault().LensModel;
                        Instance.tbFocalLength.Text = $"{query.FirstOrDefault().FocalLength} mm";
                        Instance.tbFocalLength35mm.Text = $"{query.FirstOrDefault().FocalLength35mm} mm";
                        Instance.tbExposureTime.Text = $"{query.FirstOrDefault().ExposureTime} s";
                        Instance.tbAperture.Text = query.FirstOrDefault().Aperture;
                        Instance.tbFNumber.Text = $"f/{query.FirstOrDefault().FNumber}";
                        Instance.tbDistance.Text = $"{query.FirstOrDefault().Distance} m";
                        Instance.tbISO.Text = query.FirstOrDefault().ISO;
                        Instance.tbWhiteBalance.Text = query.FirstOrDefault().WhiteBalance;
                        Instance.tbMeteringMode.Text = query.FirstOrDefault().MeteringMode;
                        Instance.tbExposureProgram.Text = query.FirstOrDefault().ExposureProgram;
                        Instance.tbColorSpace.Text = query.FirstOrDefault().ColorSpace;
                        Instance.tbXResolution.Text = query.FirstOrDefault().XResolution;
                        Instance.tbYResolution.Text = query.FirstOrDefault().YResolution;
                        Instance.tbResolutionUnit.Text = query.FirstOrDefault().ResolutionUnit;
                        Instance.tbSoftware.Text = query.FirstOrDefault().Software;
                        Instance.tbArtist.Text = query.FirstOrDefault().Artist;
                        Instance.tbCopyright.Text = query.FirstOrDefault().Copyright;
                        Instance.tbShutterSpeed.Text = query.FirstOrDefault().ShutterSpeed;
                        Instance.tbExposureBias.Text = query.FirstOrDefault().ExposureBias;
                        Instance.tbMakerNote.Text = query.FirstOrDefault().MakerNote;
                        Instance.tbUserComment.Text = query.FirstOrDefault().UserComment;
                        Instance.tbGPSVersion.Text = query.FirstOrDefault().GPSVersion;
                        Instance.tbEXIFVersion.Text = query.FirstOrDefault().EXIFVersion;

                        // TODO: populate the rest of the metadata fields.
                        // @@@
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ShowImageDetails: Error loading image details: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowImageDetails: Error retrieving photo record from database: {ex.Message}");
            }
        }

        private static void ClearImageDetails()
        {
            Instance.tbFilename.Text = string.Empty;
            Instance.tbLocation.Text = string.Empty;
            Instance.tbFileSize.Text = string.Empty;
            Instance.tbCreatedDate.Text = string.Empty;
            Instance.tbModifiedDate.Text = string.Empty;
            Instance.tbFileOwner.Text = string.Empty;
            Instance.tbTitleCaption.Text = string.Empty;
            Instance.tbDimensions.Text = string.Empty;
            Instance.tbCameraMake.Text = string.Empty;
            Instance.tbCameraModel.Text = string.Empty;
            Instance.tbImgCreationDate.Text = string.Empty;
            Instance.tbImgDigitisedDate.Text = string.Empty;
            Instance.tbOrientation.Text = string.Empty;
            Instance.tbFlash.Text = string.Empty;
            Instance.tbLens.Text = string.Empty;
            Instance.tbFocalLength.Text = string.Empty;
            Instance.tbExposureTime.Text = string.Empty;
            Instance.tbAperture.Text = string.Empty;
            Instance.tbFNumber.Text = string.Empty;
            Instance.tbDistance.Text = string.Empty;
            Instance.tbISO.Text = string.Empty;
            Instance.tbWhiteBalance.Text = string.Empty;
            Instance.tbMeteringMode.Text = string.Empty;
            Instance.tbExposureProgram.Text = string.Empty;
            Instance.tbColorSpace.Text = string.Empty;
            Instance.tbThumbnailHdr.Text = string.Empty;
            Instance.tbJPEGQuality.Text = string.Empty;
            Instance.tbUniqueID.Text = string.Empty;
            Instance.tbXResolution.Text = string.Empty;
            Instance.tbYResolution.Text = string.Empty;
            Instance.tbResolutionUnit.Text = string.Empty;
            Instance.tbSoftware.Text = string.Empty;
            Instance.tbArtist.Text = string.Empty;
            Instance.tbCopyright.Text = string.Empty;
            Instance.tbShutterSpeed.Text = string.Empty;
            Instance.tbExposureBias.Text = string.Empty;
            Instance.tbMakerNote.Text = string.Empty;
            Instance.tbUserComment.Text = string.Empty;
            Instance.tbGPSVersion.Text = string.Empty;
            Instance.tbEXIFVersion.Text = string.Empty;
        }
        #endregion MainWindowFolderList

        #region Utility_Functions
        /// <summary>
        /// Adds a hierarchical path to the specified <see cref="TreeView"/> control, creating any missing nodes.
        /// </summary>
        /// <remarks>
        /// This method splits the <paramref name="path"/> into individual folder names
        /// using the directory separator character. It then iteratively traverses or creates <see cref="TreeViewItem"/>
        /// nodes to represent each folder in the hierarchy. If a folder node already exists, it is reused; otherwise, a
        /// new node is created.
        /// </remarks>
        /// <param name="treeView">The <see cref="TreeView"/> control to which the path will be added.</param>
        /// <param name="path">The file system path to add, represented as a string with directory separators.</param>
        public static void AddPathToTree(TreeView treeView, string path)
        {
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            ItemCollection currentItems = treeView.Items;
            foreach (string folder in folders)
            {
                TreeViewItem existingItem = null;
                foreach (TreeViewItem item in currentItems)
                {
                    if ((string)item.Header == folder)
                    {
                        existingItem = item;
                        break;
                    }
                }
                if (existingItem == null)
                {
                    existingItem = new TreeViewItem
                    {
                        Header = folder,
                        Tag = folder,
                        FontWeight = FontWeights.Regular,
                        IsExpanded = true
                    };
                    currentItems.Add(existingItem);
                }
                currentItems = existingItem.Items;
            }
        }

        public static void SelectATab(TabControl tabControl, string tabName)
        {
            // Find the TabItem with the specified name and select it.
            foreach (TabItem item in tabControl.Items)
            {
                // Ensure that the TabItem has a name.
                if (string.IsNullOrEmpty(item.Name))
                {
                    throw new ArgumentException("TabItem must have a Name property set.");
                }
                else
                {
                    Debug.Print($"SelectATab: TabItem Name: '{item.Name}' / tabName: '{tabName}'");

                    if (item.Name.Equals(tabName, StringComparison.Ordinal))
                    {
                        tabControl.SelectedItem = item;
                        return; // Exit once the tab is found and selected.
                    }
                }
            }
        }

        public static void SelectAFolder(TreeView treeView, string path)
        {
            // Split the path into folders.
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            TreeViewItem node = null;
            // Traverse the TreeView to find the item that matches the path.
            foreach (string folder in folders)
            {
                if (node == null)
                {
                    node = treeView.Items.OfType<TreeViewItem>()
                        .FirstOrDefault(item => (string)item.Header == folder);
                }
                else
                {
                    node.IsExpanded = true;
                    node = node.Items.OfType<TreeViewItem>()
                        .FirstOrDefault(item => (string)item.Header == folder);
                }
                if (node == null)
                {
                    break; // Folder not found, exit loop.
                }
            }
            if (node != null)
            {
                node.IsSelected = true;
            }
        }

        /// <summary>
        /// Determine if a directory is writable by the curent process by creating and then deleting
        /// a file in that directory.  Note: unfortunately, Windows and .NET do not provide any better
        /// way to test for directory writability.
        /// </summary>
        /// <param name="dirPath">The directory to test.</param>
        /// <param name="throwIfFails">If the file creation fails, throw the error (true/false).</param>
        /// <returns></returns>
        public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()),
                                                   1, FileOptions.DeleteOnClose))
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                {
                    throw;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion Utility_Functions

        #region UICode
        /// <summary>
        /// The code in this UICode-region is the implementation of the menu bar commands.
        /// The menu items in MainWindow.xaml file are almost completely built-out to match
        /// Picasa's menus; but where a menu item is not yet implemented, it has been 
        /// disabled using the the CanExecute method.
        /// 
        /// The XAML code is configured to use the BooleanToCollapsedVisibilityConverter to
        /// determine whether or not to show the menu item.  If the menu item's CanExecute
        /// method returns false, then the menu item is hidden (collapsed).
        /// </summary>

        // AboutCmd
        private void AboutCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void AboutCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            About AboutWindow = new();
            AboutWindow.Show();
        }

        // AddFileCmd
        private void AddFileCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void AddFileCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AddFolderCmd
        private void AddFolderCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void AddFolderCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AdjustDateCmd
        private void AdjustDateCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void AdjustDateCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AutomaticCmd
        private void AutomaticCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void AutomaticCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // BackupCmd
        private void BackupCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void BackupCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // BatchUploadCmd
        private void BatchUploadCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void BatchUploadCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ConfigViewCmd
        private void ConfigViewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ConfigViewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // DeleteCmd
        private void DeleteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void DeleteCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // EditDescCmd
        private void EditDescCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void EditDescCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // EditViewCmd
        private void EditViewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void EditViewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ExitCmd
        private void ExitCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void ExitCmdExecuted(object sender, ExecutedRoutedEventArgs e) => this.Close();

        // ExportCmd
        private void ExportCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ExportCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // FolderMgrCmd
        private void FolderMgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void FolderMgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FolderManagerWindow FolderManagerWindow = new();
            FolderManagerWindow.Show();
        }

        // HelpContentsCmd
        private void HelpContentsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void HelpContentsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // HiddenPictCmd
        private void HiddenPictCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void HiddenPictCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // HideCmd
        private void HideCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void HideCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ImportFromCmd
        private void ImportFromCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ImportFromCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // LibraryViewCmd
        private void LibraryViewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void LibraryViewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // LocateFolderOnDiskCmd
        private void LocateFolderOnDiskCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void LocateFolderOnDiskCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // LocateOnDiskCmd
        private void LocateOnDiskCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void LocateOnDiskCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // MoveCmd
        private void MoveCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void MoveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // MoveFolderCmd
        private void MoveFolderCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void MoveFolderCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // NewAlbumCmd
        private void NewAlbumCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void NewAlbumCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // NrmlThumbCmd
        private void NrmlThumbCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void NrmlThumbCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // OpenFileInEditorCmd
        private void OpenFileInEditorCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void OpenFileInEditorCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // OptionsCmd
        private void OptionsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void OptionsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OptionsWindow OptionsWindow = new();
            OptionsWindow.Show();
        }

        // PeopleCmd
        private void PeopleCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void PeopleCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PeopleMgrCmd
        private void PeopleMgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void PeopleMgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PlacesCmd
        private void PlacesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void PlacesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PrintContactCmd
        private void PrintContactCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void PrintContactCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PrivacyCmd
        private void PrivacyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void PrivacyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PropertiesCmd
        private void PropertiesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void PropertiesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Determine if a photo is selected in the lbMainWindowPhotos Listbox.  If no photo
            // is selected, then show in information message box and exit; otherwise, show the
            // file's properties in an information message box.
            var selectedImage = lbMainWindowPhotos.SelectedItem;
            if (selectedImage == null)
            {
                MessageBox.Show("No photo selected.");
            }
            else
            {
                var filename = selectedImage.ToString().RmPrefix("file:///");
                // The ListItem string returns the Uri, which has slashes instead of
                // backslashes as the path separator; so, before we can use the ListItem's
                // string we have to swap the path separator back into Windows' form.
                filename = filename.Replace('/', Path.DirectorySeparatorChar);

                ViewProperties ViewPropertiesWindow = new();
                ViewPropertiesWindow.Show(filename);
            }
        }

        // ReadmeCmd
        private void ReadmeCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ReadmeCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // RefreshThumbsCmd
        private void RefreshThumbsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void RefreshThumbsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ReleaseNotesCmd
        private void ReleaseNotesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void ReleaseNotesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ReleaseNotes ReleaseNotesWindow = new();
            ReleaseNotesWindow.Show();
        }

        // RenameCmd
        private void RenameCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void RenameCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ResetFacesCmd
        private void ResetFacesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ResetFacesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // RevertCmd
        private void RevertCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void RevertCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SaveACopyCmd
        private void SaveACopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void SaveACopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SearchOptCmd
        private void SearchOptCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void SearchOptCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ShortcutsCmd
        private void ShortcutsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ShortcutsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ShowEditCtrlsCmd
        private void ShowEditCtrlsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ShowEditCtrlsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SlideshowCmd
        private void SlideshowCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void SlideshowCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SmlThumbCmd
        private void SmlThumbCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void SmlThumbCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SmallPictCmd
        private void SmallPictCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void SmallPictCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // TagsCmd
        private void TagsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void TagsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // TermsCmd
        private void TermsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void TermsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Terms TermsWindow = new();
            TermsWindow.Show();
        }

        // TimelineCmd
        private void TimelineCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void TimelineCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UndoAddEditsCmd
        private void UndoAddEditsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void UndoAddEditsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UnhideCmd
        private void UnhideCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void UnhideCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UninstallingCmd
        private void UninstallingCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void UninstallingCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UpdatesCmd
        private void UpdatesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void UpdatesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UploadMgrCmd
        private void UploadMgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void UploadMgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ViewAndEditCmd
        private void ViewAndEditCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ViewAndEditCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ViewSlidesCmd
        private void ViewSlidesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void ViewSlidesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void NavigationTabs_GotFocus(object sender, RoutedEventArgs e)
        {
            // Determine which TabItem is active and save it in Options.LastSelectedLeftTab
            if (sender is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    // Save the name of the selected tab.
                    Options.Instance.LastSelectedLeftTab = selectedTab.Name;
                    Debug.WriteLine($"NavigationTabs_GotFocus: Selected tab: {selectedTab.Name}");
                }
                else
                {
                    Debug.WriteLine("NavigationTabs_GotFocus: No tab selected.");
                }
            }
            else
            {
                Debug.WriteLine("NavigationTabs_GotFocus: sender is not a TabControl.");
            }
        }

        private void InfoTabs_GotFocus(object sender, RoutedEventArgs e)
        {
            // Determine which TabItem is active and save it in Options.LastSelectedRightTab
            if (sender is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    // Save the name of the selected tab.
                    Options.Instance.LastSelectedRightTab = selectedTab.Name;
                    Debug.WriteLine($"InfoTabs_GotFocus: Selected tab: {selectedTab.Name}");
                }
                else
                {
                    Debug.WriteLine("InfoTabs_GotFocus: No tab selected.");
                }
            }
            else
            {
                Debug.WriteLine("InfoTabs_GotFocus: sender is not a TabControl.");
            }
        }
        #endregion UICode

        /// <summary>
        /// Called when the user double-clicks on a photo.  Open the photo so that it fills
        /// the lbMainWindowPhotos panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbMainWindowPhotos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Determine what photo was double-clicked:
            // 1. Get the UI element that was the original source of the event;
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            // 2. Walk up the visual tree until a ListBoxItem is found;
            while ((dep != null) && (dep is not ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            // 3. If no ListBoxItem was found, the double click happened on empty space or
            //    scrollbar, so only take action if a ListBoxItem was found;
            if (dep != null)
            {
                // 4. Cast the found DependencyObject to a ListBoxItem; and, lastly,
                ListBoxItem clickedItemContainer = (ListBoxItem)dep;

                // 5. Use the ItemContainerGenerator to get the actual data item.
                BitmapImage clickedItem = (BitmapImage)lbMainWindowPhotos.ItemContainerGenerator.ItemFromContainer(clickedItemContainer);

                // Double-check that we got a valid data item.
                if (clickedItem != null)
                {
                    // Hide the lbMainWindowPhotos ListBox and expose the spSelectedPhoto StackPanel.
                    // I use .Hidden and not .Collapsed because I don't want the other widgets to
                    // move when the visibility is changed.
                    lbMainWindowPhotos.Visibility = Visibility.Hidden;
                    spNavTabHeader.Visibility = Visibility.Hidden;
                    grdSelectedPhoto.Visibility = Visibility.Visible;
                    btnReturnToLibrary.Visibility = Visibility.Visible;

                    // Load the photo into the imgSelectedPhoto Image widget.
                    // Note: the ListBoxItem's string is the URI of the photo.
                    imgSelectedPhoto.Source = new BitmapImage(new Uri(clickedItem.ToString(CultureInfo.InvariantCulture)));

                    // Load the photo's caption into the tbSelectedPhotoCaption TextBox.
                    tbSelectedPhotoCaption.Text
                        = GetCaptionForPhoto(new Uri(clickedItem.ToString(CultureInfo.InvariantCulture)).LocalPath);
                }
            }
        }

        /// <summary>
        /// Retrive the photo's caption from the database.  If no record is found, or if an
        /// error occurs, return an empty string.
        ///
        /// TODO: this method needs to be extended to also check in any .micasa (or .picasa)
        /// sidecar files that may exist.
        /// </summary>
        /// <param name="filename">The full pathname of the photo.</param>
        /// <returns></returns>
        private static string GetCaptionForPhoto(string filename)
        {
            string theCaption = string.Empty;

            try
            {
                var query = Instance.PhotoCol.Query()
                    .Where(x => x.FQFilename.Equals(filename, StringComparison.Ordinal));

                if (query == null || query.Count() == 0)
                {
                    Debug.WriteLine($"GetCaptionForPhoto: No database record found for selected photo: {filename}");
                    MessageBox.Show($"No database record found for selected photo: {filename}");
                }
                else if (query.Count() > 1)
                {
                    Debug.WriteLine($"GetCaptionForPhoto: Multiple database records found for selected photo: {filename}");
                    MessageBox.Show($"Multiple database records found for selected photo: {filename}");
                }
                else
                {
                    try
                    {
                        theCaption = query.FirstOrDefault().TitleCaption;
                        if (theCaption == null)
                        {
                            theCaption = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"GetCaptionForPhoto: Error loading image caption: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCaptionForPhoto: Error retrieving photo record from database: {ex.Message}");
            }
            return theCaption;
        }

        /// <summary>
        /// When the user presses the [Return to Library] button, we hide the grdSelectedPhoto
        /// StackPanel and show the lbMainWindowPhotos ListBox again.  The button itself also needs
        /// to be hidden again, and the spNavTabHeader shown again.
        ///
        /// TODO: we also need to deal with the tabs because they need to be inactive when t
        /// the user is viewing a photo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReturnToLibrary_Click(object sender, RoutedEventArgs e)
        {
            grdSelectedPhoto.Visibility = Visibility.Hidden;
            btnReturnToLibrary.Visibility = Visibility.Hidden;
            lbMainWindowPhotos.Visibility = Visibility.Visible;
            spNavTabHeader.Visibility = Visibility.Visible;
        }
    }

    #region Enums
    /// <summary>
    /// The global application mode:
    ///  * Legacy: only use Picasa sidecar files.
    ///  * Migrate: migrate data contained in Picasa sidecar files into the Micasa 
    ///    database and Micasa sidecar files.  Don't update/maintain the Picasa
    ///    sidecar files.
    ///  * Native: only use Micasa sidecar files (that is, ignore any Picasa 
    ///    sidecar files).
    /// </summary>
    [Flags]
    public enum AppMode
    {
        Legacy,
        Migrate,
        Native
    }
#endregion Enums

    public static class Constants
    {
        public const string sMcAppName = @"Micasa";

#pragma warning disable CA1707 // Identifiers should not contain underscores
        // Main Micasa .INI file -- Section: File Types
        public const string sMcFT_Section = @"File Types";
        public const string sMcFT_Avi = @".avi";
        public const string sMcFT_Bmp = @".bmp";
        public const string sMcFT_Gif = @".gif";
        public const string sMcFT_Heic = @".heic";
        public const string sMcFT_Jpg = @".jpg";
        public const string sMcFT_Mov = @".mov";
        public const string sMcFT_Nef = @".nef";
        public const string sMcFT_Png = @".png";
        public const string sMcFT_Psd = @".psd";
        public const string sMcFT_Tga = @".tga";
        public const string sMcFT_Tif = @".tif";
        public const string sMcFT_Webp = @".webp";
        // Alternate types (used in filenames, not .INI file)
        public const string sMcFT_JpgA = @".jpeg";
        public const string sMcFT_TifA = @".tiff";
#pragma warning restore CA1707 // Identifiers should not contain underscores

        // Application Saved State
        public const string sMcLastSelectedLeftTab = "LastSelectedLeftTab";
        public const string sMcLastSelectedRightTab = "LastSelectedRightTab";
        public const string sMcLastSelectedFolder = "LastSelectedFolder";
        public const string sMcLastSelectedRightFolder = "LastSelectedRightFolder";

        // Application Options
        public const string sMcOpAppMode = @"AppMode";
        public const string sMcUpdPhotoFiles = @"UpdatePhotoFiles";

        // .Micasa -- Entries for each folder
        public const string sMcScMicasa = @"Micasa";

        // .Picasa -- Entries for each folder
        public const string sMcScPicasa = @"Picasa";

        // .Micasa and .Picasa files -- Entries for each photo
        public const string sMcPhCaption = @"caption";
        public const string sMcPhStar = @"star";
        public const string sMcPhAlbum = @"albums";

        // .Micasa and .Picasa files -- Filenames.
        public const string sMcDotMicasa = @".Micasa";
        public const string sMcDotPicasa = @".picasa";
        public const string sMcPicasaIni = @"Picasa";

        // .Micasa and .Picasa files -- Entries for each Micasa/Picasa section
        public const string sMcAlbumRef = @".albums:";
        public const string sMcName = @"name";
        public const string sMcToken = @"token";
        public const string sMcDate = @"date";

        // Micasa watch lists.
        public const string sMcWatchedFileNm = @".Micasa.WatchedFolders.txt";
        public const string sMcOneTimeFileNm = @".Micasa.OneTimeFolders.txt";
        public const string sMcExcludeFileNm = @".Micasa.ExcludeFolders.txt";

        // Databases
        public const string sMcAppDataFolder = @"Micasa";
        public const string sMcMicasaDBFileNm = @"Micasa.db";
        public const string sMcFoldersColNm = @"Folders";
        public const string sMcPhotosColNm = @"Photos";

        // Date to use when no date (or an invalid date) is found.  Note: C# doesn't
        // support DateTime constants so we've coded a readonly object.
        public static readonly DateTime UnixEpoch = new(1970, 1, 1);

        // Micassa Options Panel -- Database Rebuild
        public const string sMcRebuildConfirm = @"YES";
        public const string sMcRebuildInstrRTF = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}}{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;}{\*\generator Riched20 10.0.19041}\viewkind4\uc1 \pard\sa200\sl276\slmult1\cf1\f0\fs20\lang9 To rebuild the Micasa local database, type '" + sMcRebuildConfirm + @"' (no quotes) in the text box and press [Rebuild].\line\cf2\b Caution:\cf1\b0  the rebuild \b cannot be stopped\b0  once it has been started!\par}";

        // Micasa About panel.  This section is at the end of the file to make it 
        // easy to find, to update the version information.
        public const string sMcMainSection = @"Main";
        public const string sMcVersion = @"0.12"; // --> Also update in "AssemblyInfo.cs" <--
        public const string sMcPlatform = @"Windows 11";
        public const string sMcCopyright = @"2021{\'96}2026"; // "{\'96}" is an RTF en dash character.
        public const string sVersionToRepl = @"[@Version String@]";
        public const string sPlatformToRepl = @"[@Platform String@]";
        public const string sCopyrightToRepl = @"[@Copyright Date@]";
        public const string sAppIniFileNm = @".Micasa.ini";
        public const string sAuthorEmail = @"christopher@rath.ca";
    }
}
