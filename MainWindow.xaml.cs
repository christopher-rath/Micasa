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
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.ComponentModel;

namespace Micasa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } 
        private static readonly DeletedScanner _DeletedScanner = new();
        private static CancellationTokenSource DeletedScannerCancellationSource = new();
        private static CancellationToken DeletedScannerCancellationToken = DeletedScannerCancellationSource.Token;
        private static readonly PictureScanner _PictureScanner = new();
        private static CancellationTokenSource PictureScannerCancellationSource = new();
        private static CancellationToken PictureScannerCancellationToken = PictureScannerCancellationSource.Token;
        private static readonly string _AppData = Environment.ExpandEnvironmentVariables(@"%APPDATA%");
        private static readonly PictureWatcher _ActiveWatchers = new();
        private static CancellationTokenSource PictureProcessorCancellationSource = new();
        private static CancellationToken PictureProcessorCancellationToken = PictureProcessorCancellationSource.Token;

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

            try
            {
                Directory.CreateDirectory(AppData + System.IO.Path.DirectorySeparatorChar + Constants.sMcAppDataFolder);
            }
            catch
            {
                string msg = "ERROR: unable to create Micasa roaming folder (" + AppData + System.IO.Path.DirectorySeparatorChar
                            + Constants.sMcAppDataFolder + ").\n\nUnable to continue.";
                MessageBox.Show(msg, "Roaming Folder Creation Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }

            try
            {
                Database.CreateDB();
                StartScanners();
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.Message, "Unexpected ArgumentException Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }
            catch (IOException e)
            {
                string msg = @"Unexpected IOException error opening the Micasa database (" + e.Message + @").";
                MessageBox.Show(msg, "Database Open Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }

            try
            {
                StartWatchers();
            }
            catch (Exception e)
            {
                string msg = $"ERROR: unexpected error setting up FileSystemWatchers for watched folders: {e.Message}\n\nUnable to continue.";
                MessageBox.Show(msg, "FileSystemWatcher Creation Error", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                this.Close();
            }
        }

        #region GetterSetters
        public static string AppData
        {
            get { return _AppData; }
        }
        #endregion GetterSetters

        #region Event_Handlers
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stopscanners();
            _ActiveWatchers.StopWatchers();
        }

        //private void Window_ContentRendered(object sender, EventArgs e)
        //{
        //}
        #endregion Event_Handlers

        #region Thread_Code

        /// <summary>
        /// Start the scanners.
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
        /// Stop the scanners.
        /// </summary>
        public static void Stopscanners()
        {
            PictureScannerCancellationSource.Cancel();
            DeletedScannerCancellationSource.Cancel();
        }

        /// <summary>
        /// Start the watchers; one for each folder in the WatchedFolders list.
        /// Then start the associated processor.
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

        #region Utility_Functions
        /// <summary>
        /// Determine if a directory is writable by the curent process by creating and then deleting
        /// a file in that directory.
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
        // AboutCmd
        private void AboutCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AboutCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            About AboutWindow = new();
            AboutWindow.Show();
        }

        // AddFileCmd
        private void AddFileCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void AddFileCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AddFolderCmd
        private void AddFolderCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void AddFolderCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AddToScrnSvrCmd
        private void AddToScrnSvrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void AddToScrnSvrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AdjustDateCmd
        private void AdjustDateCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void AdjustDateCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // AutomaticCmd
        private void AutomaticCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void AutomaticCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // BackupCmd
        private void BackupCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BackupCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // BatchUploadCmd
        private void BatchUploadCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BatchUploadCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ConfigButtonsCmd
        private void ConfigButtonsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ConfigButtonsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ConfigScrnSavCmd
        private void ConfigScrnSavCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ConfigScrnSavCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ConfigViewCmd
        private void ConfigViewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ConfigViewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // CreateGCDCmd
        private void CreateGCDCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void CreateGCDCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // DeleteCmd
        private void DeleteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void DeleteCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // EditViewCmd
        private void EditViewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void EditViewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // EditDescCmd
        private void EditDescCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void EditDescCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // EmailCmd
        private void EmailCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void EmailCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ExitCmd
        private void ExitCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        // ExportCmd
        private void ExportCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ExportCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ExportHTMLCmd
        private void ExportHTMLCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ExportHTMLCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ExportToDVRCmd
        private void ExportToDVRCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ExportToDVRCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // FolderMgrCmd
        private void FolderMgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FolderMgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FolderManagerWindow FolderManagerWindow = new();
            FolderManagerWindow.Show();
        }

        // ForumsCmd
        private void ForumsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ForumsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // HelpContentsCmd
        private void HelpContentsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void HelpContentsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // HideCmd
        private void HideCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void HideCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // HiddenPictCmd
        private void HiddenPictCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void HiddenPictCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ImportFromCmd
        private void ImportFromCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ImportFromCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // LocateFolderOnDiskCmd
        private void LocateFolderOnDiskCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void LocateFolderOnDiskCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // LibraryViewCmd
        private void LibraryViewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void LibraryViewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // LocateOnDiskCmd
        private void LocateOnDiskCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void LocateOnDiskCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // MakeAPosterCmd
        private void MakeAPosterCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void MakeAPosterCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // MoveCmd
        private void MoveCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void MoveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // MoveFolderCmd
        private void MoveFolderCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void MoveFolderCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // NewAlbumCmd
        private void NewAlbumCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void NewAlbumCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // NrmlThumbCmd
        private void NrmlThumbCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void NrmlThumbCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // OpenFileInEditorCmd
        private void OpenFileInEditorCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void OpenFileInEditorCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // OptionsCmd
        private void OptionsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OptionsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OptionsWindow OptionsWindow = new();
            OptionsWindow.Show();
        }

        // PeopleCmd
        private void PeopleCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PeopleCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PeopleMgrCmd
        private void PeopleMgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PeopleMgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PictureCollageCmd
        private void PictureCollageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PictureCollageCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PlacesCmd
        private void PlacesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PlacesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PrintContactCmd
        private void PrintContactCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PrintContactCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PrivacyCmd
        private void PrivacyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PrivacyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PropertiesCmd
        private void PropertiesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PropertiesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // PublishToBlgrCmd
        private void PublishToBlgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void PublishToBlgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ReadmeCmd
        private void ReadmeCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ReadmeCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // RefreshThumbsCmd
        private void RefreshThumbsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void RefreshThumbsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ReleaseNotesCmd
        private void ReleaseNotesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ReleaseNotesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ReleaseNotes ReleaseNotesWindow = new();
            ReleaseNotesWindow.Show();
        }

        // RemoveFromCmd
        private void RemoveFromCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void RemoveFromCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // RenameCmd
        private void RenameCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void RenameCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ResetFacesCmd
        private void ResetFacesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ResetFacesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // RevertCmd
        private void RevertCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void RevertCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SaveACopyCmd
        private void SaveACopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SaveACopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SearchOptCmd
        private void SearchOptCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SearchOptCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SetAsDesktopCmd
        private void SetAsDesktopCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SetAsDesktopCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ShortcutsCmd
        private void ShortcutsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ShortcutsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ShowEditCtrlsCmd
        private void ShowEditCtrlsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ShowEditCtrlsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SlideshowCmd
        private void SlideshowCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SlideshowCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SmlThumbCmd
        private void SmlThumbCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SmlThumbCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // SmallPictCmd
        private void SmallPictCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SmallPictCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // TagsCmd
        private void TagsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void TagsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // TermsCmd
        private void TermsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void TermsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Terms TermsWindow = new();
            TermsWindow.Show();
        }

        // TimelineCmd
        private void TimelineCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void TimelineCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UninstallingCmd
        private void UninstallingCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void UninstallingCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UpdatesCmd
        private void UpdatesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void UpdatesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UploadMgrCmd
        private void UploadMgrCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void UploadMgrCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UnhideCmd
        private void UnhideCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void UnhideCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UseClrMgmtCmd
        private void UseClrMgmtCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void UseClrMgmtCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ViewAndEditCmd
        private void ViewAndEditCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ViewAndEditCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // ViewSlidesCmd
        private void ViewSlidesCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void ViewSlidesCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        // UndoAddEditsCmd
        private void UndoAddEditsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void UndoAddEditsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        #endregion UICode
    }

    [Flags]
    public enum AppMode
    {
        Legacy,
        Migrate,
        Native
    }

    public static class Constants
    {
        public const string sMcAppName = @"Micasa";
#pragma warning disable CA1707 // Identifiers should not contain underscores
        // Main Micasa .INI file -- Section: File Types
        public const string sMcFT_Section = @"File Types";
        public const string sMcFT_Avi = @".avi";
        public const string sMcFT_Bmp = @".bmp";
        public const string sMcFT_Gif = @".gif";
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
        public const string sMcRebuildInstrRTF = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}}{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;}{\*\generator Riched20 10.0.19041}\viewkind4\uc1 \pard\sa200\sl276\slmult1\cf1\f0\fs20\lang9 To rebuild the Micasa local database, type 'YES' (no quotes) in the text box and press [Rebuild].\line\cf2\b Caution:\cf1\b0  the rebuild \b cannot be stopped\b0  once it has been started!\par}";
        public const string sMcRebuildConfirm = @"YES";

        // Micasa About panel.  This section is at the end of the file to make it 
        // easy to find, to update the version information.
        public const string sMcMainSection = @"Main";
        public const string sMcVersion = @"0.07"; // --> Also update in "AssemblyInfo.cs" <--
        public const string sMcPlatform = @"Windows 10";
        public const string sMcCopyright = @"2021{\'96}2024"; // "{\'96}" is an RTF en dash character.
        public const string sVersionToRepl = @"[@Version String@]";
        public const string sPlatformToRepl = @"[@Platform String@]";
        public const string sCopyrightToRepl = @"[@Copyright Date@]";
        public const string sAppIniFileNm = @".Micasa.ini";
        public const string sAuthorEmail = @"christopher@rath.ca";
    }
}
