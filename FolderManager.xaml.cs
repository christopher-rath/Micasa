#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2022 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Collections.Generic;
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
using System.IO;
using StringExtensions;

namespace Micasa
{
    /// <summary>
    /// Interaction logic for FolderManager.xaml
    /// </summary>
    public partial class FolderManagerWindow : Window
    {
        private object dummyNode = null;
        private Dictionary<string, WatchType> _SavedFolderList = new();
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

            // Stop the picture scanner while we have this dialog box open.
            MainWindow.Stopscanners();

            // Load special folders before loading the filesystem.
            anItem = new TreeViewItem();
            anItem.Header = WatchedLists.ThisPCStr;
            anItem.Tag = WatchedLists.ThisPCStr + WatchedLists.ComputerPath;
            anItem.FontWeight = FontWeights.Bold;
            anItem.Items.Add(dummyNode);
            anItem.Expanded += new RoutedEventHandler(folder_Expanded);
            //anItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
            foldersItem.Items.Add(anItem);
            myCompItem = (TreeViewItem)foldersItem.Items[0];

            // Now load the file system items...
            foreach (string s in Directory.GetLogicalDrives())
            {
                anItem = new TreeViewItem();
                anItem.Header = s;
                anItem.Tag = s;
                anItem.FontWeight = FontWeights.Bold;
                anItem.Items.Add(dummyNode);
                anItem.Expanded += new RoutedEventHandler(folder_Expanded);
                //anItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                foldersItem.Items.Add(anItem);
            }
            myCompItem.IsExpanded = true;
            foreach (var item in myCompItem.Items)
            {
                anItem = (TreeViewItem)item;
                anItem.IsExpanded = true;
            }

            // TODO: change these fields from Text to lists that will support having a
            // user click on an entry and causing Micasa to open the folder list to 
            // that location.
            tbWatchedFolders.Text = string.Join("\n", WatchedLists.Instance.WatchedFolders);
            tbOneTimeFolders.Text = string.Join("\n", WatchedLists.Instance.OnetimeFolders);
            tbExcludeFolders.Text = string.Join("\n", WatchedLists.Instance.ExcludedFolders);

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

        // --- Commented out for the time-being.  I no longer need
        // --- to have all child items be deleted when the now is
        // --- collapsed.
        //void folder_Collapsed(object sender, RoutedEventArgs e)
        //{
        //    TreeViewItem item = (TreeViewItem)e.OriginalSource;
        //
        //    item.Items.Clear();
        //    item.Items.Add(dummyNode);
        //}

        void folder_Expanded(object sender, RoutedEventArgs e)
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
                            & !s.IsSpecialDir() & isThisPC_OKtoList(item, s))
                        {
                            TreeViewItem subitem = new();
                            subitem.Header = s.Substring(s.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
                            subitem.Tag = s;
                            subitem.FontWeight = FontWeights.Normal;
                            subitem.Items.Add(dummyNode);
                            if (isThisPC_Item(item))
                            {
                                subitem.Expanded += new RoutedEventHandler(folder_ExpandedThisPC);
                                //subitem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                            }
                            else
                            {
                                subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                                //subitem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                            }
                            item.Items.Add(subitem);
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        void folder_ExpandedThisPC(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
            }
        }

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem item = ((TreeViewItem)tree.SelectedItem);
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
            if (isThisPC_Item(item))
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

        private void rbOneTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!blindSet && SelectedFolderSaved != null)
            {
                if (WatchedLists.Instance.FolderList.ContainsKey(SelectedFolderSaved))
                {
                    WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                }
                WatchedLists.Instance.FolderList.Add(SelectedFolderSaved, WatchType.Onetime);
                tbWatchedFolders.Text = string.Join("\n", WatchedLists.Instance.WatchedFolders);
                tbOneTimeFolders.Text = string.Join("\n", WatchedLists.Instance.OnetimeFolders);
                tbExcludeFolders.Text = string.Join("\n", WatchedLists.Instance.ExcludedFolders);
                replaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

        private void rbExclude_Checked(object sender, RoutedEventArgs e)
        {
            if (!blindSet && SelectedFolderSaved != null)
            {
                if (WatchedLists.Instance.FolderList.ContainsKey(SelectedFolderSaved))
                {
                    WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                }
                WatchedLists.Instance.FolderList.Add(SelectedFolderSaved, WatchType.Excluded);
                tbWatchedFolders.Text = string.Join("\n", WatchedLists.Instance.WatchedFolders);
                tbOneTimeFolders.Text = string.Join("\n", WatchedLists.Instance.OnetimeFolders);
                tbExcludeFolders.Text = string.Join("\n", WatchedLists.Instance.ExcludedFolders);
                replaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

        private void rbMonitor_Checked(object sender, RoutedEventArgs e)
        {
            if (!blindSet && SelectedFolderSaved != null)
            {
                if (WatchedLists.Instance.FolderList.ContainsKey(SelectedFolderSaved))
                {
                    WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                }
                WatchedLists.Instance.FolderList.Add(SelectedFolderSaved, WatchType.Watched);
                tbWatchedFolders.Text = string.Join("\n", WatchedLists.Instance.WatchedFolders);
                tbOneTimeFolders.Text = string.Join("\n", WatchedLists.Instance.OnetimeFolders);
                tbExcludeFolders.Text = string.Join("\n", WatchedLists.Instance.ExcludedFolders);
                replaceItemWithClone(SelectedItem);
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
            this.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFolderSaved != null)
            {
                if (WatchedLists.Instance.FolderList.ContainsKey(SelectedFolderSaved))
                {
                    WatchedLists.Instance.FolderList.Remove(SelectedFolderSaved);
                }
                tbWatchedFolders.Text = string.Join("\n", WatchedLists.Instance.WatchedFolders);
                tbOneTimeFolders.Text = string.Join("\n", WatchedLists.Instance.OnetimeFolders);
                tbExcludeFolders.Text = string.Join("\n", WatchedLists.Instance.ExcludedFolders);
                replaceItemWithClone(SelectedItem);
                foldersItem.Focus();
            }
        }

        private bool isThisPC_OKtoList(TreeViewItem item, string path)
        {
            bool rtnVal = true;

            // We only have to do checks if the TreeViewItem that is enumerating the directories
            // is a ThisPC item.
            if (isThisPC_Item(item))
            {
                if (!(path.Equals(WatchedLists.DesktopPath)
                      || path.Equals(WatchedLists.DocumentsPath)
                      || path.Equals(WatchedLists.PicturesPath)))
                {
                    // If path wasn't one of the special foldernames tested for in the if() then we 
                    // return false.
                    rtnVal = false;
                }
            }
            return rtnVal;
        }

        private bool isThisPC_Item(TreeViewItem item)
        {
            bool rtnVal = false;

            if (item.Header.Equals(WatchedLists.ThisPCStr))
            {
                rtnVal = true;
            }
            return rtnVal;
        }

        private void replaceItemWithClone(TreeViewItem item)
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
                newItem.Expanded += new RoutedEventHandler(folder_Expanded);
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
                if (isThisPC_Item(destItem)) 
                {
                    newItem.Expanded += new RoutedEventHandler(folder_ExpandedThisPC);
                    //newItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                }
                else
                {
                    newItem.Expanded += new RoutedEventHandler(folder_Expanded);
                    //newItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                }
                newItem.IsSelected = true;
                SelectedItem = newItem;
                destItem.Items[index] = newItem;
            }
        }
    }
}
