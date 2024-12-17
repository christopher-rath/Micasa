#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2025 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using StringExtensions;
using System.Diagnostics;
using Path = System.IO.Path;

namespace Micasa
{
    /// <summary>
    /// Interaction logic for FolderManager.xaml
    /// </summary>
    public partial class FolderManagerWindow : Window
    {
        private readonly object dummyNode = null;
        private readonly Dictionary<string, WatchType> _SavedFolderList = [];
        private TreeViewItem SelectedItem = null;
        private string SelectedFolderSaved = null;
        // Allow changing of how the rbMonitor radio button list is displayed, without
        // triggering the code that updates the folder lists.
        private bool blindSet = false;

        public FolderManagerWindow()
        {
            InitializeComponent();
        }

        public string SelectedImagePath { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewItem anItem = null;
            TreeViewItem myCompItem = null;

            // Stop the picture scanners and folder watchers while we have this dialog
            // box open.
            MainWindow.Stopscanners();
            MainWindow.StopWatchers();

            // Load special folders before loading the filesystem.
            anItem = new TreeViewItem
            {
                Header = WatchedLists.ThisPCStr,
                Tag = WatchedLists.ThisPCStr + WatchedLists.ComputerPath,
                FontWeight = FontWeights.Bold
            };
            anItem.Items.Add(dummyNode);
            anItem.Expanded += new RoutedEventHandler(Folder_Expanded);
            //anItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
            foldersItem.Items.Add(anItem);
            myCompItem = (TreeViewItem)foldersItem.Items[0];

            // Now load the file system items...
            foreach (var d in System.IO.DriveInfo.GetDrives())
            {
                var s = d.Name;

                anItem = new TreeViewItem
                {
                    Header = s,
                    Tag = s,
                    FontWeight = FontWeights.Bold
                };
                // Skip over unknown drives and drives with no root directory -- we aren't able to handle them.
                if (d.DriveType is not DriveType.Unknown and not DriveType.NoRootDirectory)
                {
                    if (d.DriveType is DriveType.Removable or DriveType.CDRom or DriveType.Network)
                    {
                        // At the moment, Micasa can't handle Removable, CD-ROM, or Network drives; so disable the entry.
                        anItem.IsEnabled = false;
                        anItem.FontWeight = FontWeights.Normal;
                    }
                    anItem.Items.Add(dummyNode);
                    anItem.Expanded += new RoutedEventHandler(Folder_Expanded);
                    //anItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                    foldersItem.Items.Add(anItem);
                }
            }
            myCompItem.IsExpanded = true;
            foreach (var item in myCompItem.Items)
            {
                anItem = (TreeViewItem)item;
                anItem.IsExpanded = true;
            }

            lbWatchedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.WatchedFolders);
            lbOneTimeFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.OnetimeFolders);
            lbExcludedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.ExcludedFolders);

            // Since no folder is selected, disable the buttons.  They get enabled when a 
            // folder is selected.
            rbOneTime.IsEnabled = false;
            rbExclude.IsEnabled = false;
            rbMonitor.IsEnabled = false;
            Reset.IsEnabled = false;

            // We make a private copy of the FolderList dictionary that we can
            // restore if the user presses [Cancel].
            lock (WatchedLists.Instance.WatchedListsWriteLock)
            {
                WatchedLists.Instance.WriteLocked = true;
            }
            foreach (var item in WatchedLists.Instance.FolderList)
            {
                _SavedFolderList.Add(item.Key, item.Value);
            }
        }

        #region Folder Manager Listboxes
        public class FolderItem
        {
            public string Foldername { get; set; }
        }

        static List<FolderItem> PopulateFolderListbox(string[] TheFolders)
        {
            List<FolderItem> FoldersList = new();
            foreach (string item in TheFolders)
            {
                FoldersList.Add(new FolderItem() { Foldername = item });
            }
            return FoldersList;
        }

        private void FolderListItemSelected(object sender, RoutedEventArgs e)
        {
            string thePath = ((e.Source as ListBoxItem).Content as FolderItem).Foldername;

            if (thePath != null)
            {
                bool firstScan = true;
                string aFolder = "";
                string[] folders = thePath.Split(Path.DirectorySeparatorChar);
                TreeViewItem nodeToScan = null;

                Debug.WriteLine("Folder Manager dialog: " + thePath + " folder was selected.");

                foreach (string folder in folders)
                {
                    aFolder = folder;
                    if (firstScan)
                    {
                        firstScan = false;
                        // The scan of the level 1 nodes is different than all the others.
                        if (aFolder.EndsWith(':'))
                        {
                            aFolder += @"\";
                        }
                        foreach (TreeViewItem node in foldersItem.Items)
                        {
                            if (string.Equals((string)node.Header, aFolder, StringComparison.Ordinal))
                            {
                                node.IsExpanded = true;
                                node.IsSelected = true;
                                // Remember which node we found.
                                nodeToScan = node;
                                // Once we've found the entry there is no point in looking further.
                                break;
                            }
                        }
                    } 
                    else
                    {
                        // This is a second or subsequent scan.
                        foreach (TreeViewItem node in nodeToScan.Items)
                        {
                            if (string.Equals((string)node.Header, aFolder, StringComparison.Ordinal))
                            {
                                node.IsExpanded = true;
                                node.IsSelected = true;
                                node.BringIntoView();
                                // Remember which node we found.
                                nodeToScan = node;
                                // Once we've found the entry there is no point in looking further.
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        // --- Commented out for the time-being.  I no longer need
        // --- to have all child items be deleted when the node is
        // --- collapsed.
        //void folder_Collapsed(object sender, RoutedEventArgs e)
        //{
        //    TreeViewItem item = (TreeViewItem)e.OriginalSource;
        //
        //    item.Items.Clear();
        //    item.Items.Add(dummyNode);
        //}

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            bool showHidden = false;
            bool showSystem = false;

            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    string dirs = item.Tag.ToString().RmPrefix(WatchedLists.ThisPCStr);

                    foreach (string s in Directory.GetDirectories(dirs))
                    {
                        FileAttributes theFAs = File.GetAttributes(s);

                        // This complicated bit of code ensures that we only display hidden or system folders
                        // if the applicable flag is set; plus special folders are always not on display.
                        if (((((theFAs & FileAttributes.Hidden) == FileAttributes.Hidden) & showHidden)
                                || !((theFAs & FileAttributes.Hidden) == FileAttributes.Hidden))
                            & ((((theFAs & FileAttributes.System) == FileAttributes.System) & showSystem)
                                || !((theFAs & FileAttributes.System) == FileAttributes.System))
                            & !s.IsSpecialDir() & IsThisPC_OKtoList(item, s))
                        {
                            TreeViewItem subitem = new()
                            {
                                Header = s[(s.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1)..],
                                Tag = s,
                                FontWeight = FontWeights.Normal
                            };
                            subitem.Items.Add(dummyNode);
                            if (IsThisPC_Item(item))
                            {
                                subitem.Expanded += new RoutedEventHandler(Folder_ExpandedThisPC);
                                //subitem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                            }
                            else
                            {
                                subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                                //subitem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                            }
                            item.Items.Add(subitem);
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        private void Folder_ExpandedThisPC(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
            }
        }

        private void FoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem item = (TreeViewItem)tree.SelectedItem;
            string path = ((string)item.Tag).RmPrefix(WatchedLists.ThisPCStr);

            SelectedItem = item;
            #region TreeWalkExample
            // This code shows how to walk back up the tree structure to 
            // assemble a full "path" from the root to the selected node;
            // however, that is not necessary in Micasa becuase we assign
            // the full path in the ListViewITem.Tag property.
            //if (item != null)
            //{
            //    SelectedImagePath = "";
            //    string temp1 = "";
            //    string temp2 = "";

            //    while (true)
            //    {
            //        temp1 = item.Header.ToString();
            //        if (temp1.Contains(System.IO.Path.DirectorySeparatorChar))
            //        {
            //            temp2 = "";
            //        }
            //        SelectedImagePath = temp1 + temp2 + SelectedImagePath;
            //        if (item.Parent.GetType().Equals(typeof(TreeView)))
            //        {
            //            break;
            //        }
            //        item = ((TreeViewItem)item.Parent);
            //        temp2 = System.IO.Path.DirectorySeparatorChar;
            //    }
            //// Show user selected path
            //MessageBox.Show(SelectedImagePath);
            //}
            #endregion

            SelectedFolderSaved = path;
            // Set blindSet = true so that as tha rbMonitor values are set for display
            // purposes, that Micasa doesn't make any attempt to change the folder lists.
            blindSet = true;
            switch (WatchedLists.FolderDisposition(path))
            {
                case WatchType.Watched:
                    rbMonitor.IsChecked = true;
                    break;
                case WatchType.Onetime:
                    rbOneTime.IsChecked = true;
                    break;
                case WatchType.Excluded:
                    rbExclude.IsChecked = true;
                    break;
                default:
                    rbExclude.IsChecked = true;
                    break;
            }
            blindSet = false;
            if (IsThisPC_Item(item))
            {
                rbOneTime.IsEnabled = false;
                rbExclude.IsEnabled = false;
                rbMonitor.IsEnabled = false;
                Reset.IsEnabled = false;
            }
            else
            {
                rbOneTime.IsEnabled = true;
                rbExclude.IsEnabled = true;
                rbMonitor.IsEnabled = true;
                Reset.IsEnabled = true;
            }

            return;
        }

#pragma warning disable IDE1006 // Naming Styles
        private void rbOneTime_Checked(object sender, RoutedEventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (!blindSet && SelectedFolderSaved != null)
            {
                WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                WatchedLists.Instance.FolderList.Add(SelectedFolderSaved, WatchType.Onetime);
                lbWatchedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.WatchedFolders);
                lbOneTimeFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.OnetimeFolders);
                lbExcludedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.ExcludedFolders);
                ReplaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        private void rbExclude_Checked(object sender, RoutedEventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (!blindSet && SelectedFolderSaved != null)
            {
                WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                WatchedLists.Instance.FolderList.Add(SelectedFolderSaved, WatchType.Excluded);
                lbWatchedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.WatchedFolders);
                lbOneTimeFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.OnetimeFolders);
                lbExcludedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.ExcludedFolders);
                ReplaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        private void rbMonitor_Checked(object sender, RoutedEventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (!blindSet && SelectedFolderSaved != null)
            {
                WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                WatchedLists.Instance.FolderList.Add(SelectedFolderSaved, WatchType.Watched);
                lbWatchedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.WatchedFolders);
                lbOneTimeFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.OnetimeFolders);
                lbExcludedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.ExcludedFolders);
                ReplaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            WatchedLists.Instance.WriteWatchFiles();
            lock (WatchedLists.Instance.WatchedListsWriteLock)
            {
                WatchedLists.Instance.WriteLocked = false;
            }
            // Restart the scanner as we close the dialog box.
            MainWindow.StartScanners();
            MainWindow.StopWatchers();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Restore the saved copy of the Dictionary.
            WatchedLists.Instance.FolderList.Clear();
            foreach (var item in _SavedFolderList)
            {
                WatchedLists.Instance.FolderList.Add(item.Key, item.Value);
                WatchedLists.Instance.WriteWatchFiles();
            }
            lock (WatchedLists.Instance.WatchedListsWriteLock)
            {
                WatchedLists.Instance.WriteLocked = false;
            }
            // Restart the scanner as we close the dialog box.
            MainWindow.StartScanners();
            MainWindow.StartWatchers();
            this.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFolderSaved != null)
            {
                WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                lbWatchedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.WatchedFolders);
                lbOneTimeFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.OnetimeFolders);
                lbExcludedFolders.ItemsSource = PopulateFolderListbox(WatchedLists.Instance.ExcludedFolders);
                ReplaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

        private static bool IsThisPC_OKtoList(TreeViewItem item, string path)
        {
            bool rtnVal = true;

            // We only have to do checks if the TreeViewItem that is enumerating the directories
            // is a ThisPC item.
            if (IsThisPC_Item(item))
            {
                if (!(path.Equals(WatchedLists.DesktopPath, StringComparison.Ordinal)
                      || path.Equals(WatchedLists.DocumentsPath, StringComparison.Ordinal)
                      || path.Equals(WatchedLists.PicturesPath, StringComparison.Ordinal)))
                {
                    // If path wasn't one of the special foldernames tested for in the if() then we 
                    // return false.
                    rtnVal = false;
                }
            }
            return rtnVal;
        }

        private static bool IsThisPC_Item(TreeViewItem item)
        {
            bool rtnVal = false;

            if (item.Header.Equals(WatchedLists.ThisPCStr))
            {
                rtnVal = true;
            }
            return rtnVal;
        }

        private void ReplaceItemWithClone(TreeViewItem item)
        {
            TreeViewItem newItem = new();

            if (item.Parent.GetType().Equals(typeof(TreeView)))
            {
                TreeView destItem = (TreeView)item.Parent;
                int index = destItem.Items.IndexOf(SelectedItem);

                newItem.Header = SelectedItem.Header;
                newItem.Tag = SelectedItem.Tag;
                newItem.FontWeight = SelectedItem.FontWeight;
                newItem.Items.Add(dummyNode);
                newItem.Expanded += new RoutedEventHandler(Folder_Expanded);
                //newItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                newItem.IsSelected = true;
                SelectedItem = newItem;
                destItem.Items[index] = newItem;
            }
            else
            {
                TreeViewItem destItem = (TreeViewItem)item.Parent;
                int index = destItem.Items.IndexOf(SelectedItem);

                newItem.Header = SelectedItem.Header;
                newItem.Tag = SelectedItem.Tag;
                newItem.FontWeight = SelectedItem.FontWeight;
                newItem.Items.Add(dummyNode);
                if (IsThisPC_Item(destItem)) 
                {
                    newItem.Expanded += new RoutedEventHandler(Folder_ExpandedThisPC);
                    //newItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                }
                else
                {
                    newItem.Expanded += new RoutedEventHandler(Folder_Expanded);
                    //newItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                }
                newItem.IsSelected = true;
                SelectedItem = newItem;
                destItem.Items[index] = newItem;
            }
        }
    }
}
