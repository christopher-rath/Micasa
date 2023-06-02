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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Micasa
{
    public enum WatchType
    { 
        Onetime,
        Watched,
        Excluded
    }

    public class WatchedLists
    {
        public static readonly string watchedListFilename = Options.HomeFolder + Path.DirectorySeparatorChar + Constants.sMcWatchedFileNm;
        public static readonly string excludeListFilename = Options.HomeFolder + Path.DirectorySeparatorChar + Constants.sMcExcludeFileNm;
        public static readonly string oneTimeListFilename = Options.HomeFolder + Path.DirectorySeparatorChar + Constants.sMcOneTimeFileNm;
        // Special Folders.
        public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static readonly string PicturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        // ComputerPath is manually calculated because in .NET 
        // Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) will always 
        // return an empty string.
        public static readonly string ComputerPath = System.IO.Path.GetDirectoryName(DesktopPath);
        public static readonly string ThisPCStr = " > This PC > ";
        private Dictionary<string, WatchType> _FolderList = new();
        private bool _WriteLocked = false;
#pragma warning disable CA2211 // Non-constant fields should not be visible
        // The single instance of WatchedLists (Singleton design pattern).
        public static WatchedLists Instance = new();
#pragma warning restore CA2211 // Non-constant fields should not be visible
        // Locks
        private readonly object watchedListsWriteLock = new();

        /// <summary>
        /// This WatchedLists class uses private constructor to implement the Singleton design pattern.  
        /// The single instance of this class is referenced via WatchedLists.Instance.
        /// </summary>
        private WatchedLists()
        { 
            string[] emptyFile = { "" };
            StreamReader reader;

            // We'll initalise each file so that the code that follows doesn't need to worry
            // about whether files exist.
            if (!File.Exists(watchedListFilename))
            {
                File.WriteAllLines(watchedListFilename, emptyFile);
            }
            if (!File.Exists(oneTimeListFilename))
            {
                File.WriteAllLines(oneTimeListFilename, emptyFile);
            }
            if (!File.Exists(excludeListFilename))
            {
                File.WriteAllLines(excludeListFilename, emptyFile);
            }

            // Now that we've initialised the files, we load any content into the FolderList 
            // dictionary.  We load the Excluded list first, then Onetime, then Watched; and, 
            // if each "load" activiy encouters a conflict we remove the preceding entry and
            // make the new entry the master.
            //
            // Note: this means that Watched folders take precedence over Onetime folders,
            // which take precendence over Excluded folders.  TODO: determine if Excluded
            // folders should take precedence over the other two.
            using (reader = new StreamReader(excludeListFilename))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    if (0 < line.Length)
                    {
                        if (IsPathFormatOK(line)) 
                        { 
                            FolderList.Remove(line);
                            FolderList.Add(line, WatchType.Excluded);
                        } else
                        {
                            Debug.WriteLine("Discarded invalid path (" + line + ") from " + excludeListFilename + ".");
                        }
                    }
                }
            }
            using (reader = new StreamReader(oneTimeListFilename))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    if (0 < line.Length)
                    {
                        if (IsPathFormatOK(line))
                        {
                            FolderList.Remove(line);
                            FolderList.Add(line, WatchType.Onetime);
                        }
                        else
                        {
                            Debug.WriteLine("Discarded invalid path (" + line + ") from " + oneTimeListFilename + ".");
                        }
                    }
                }
            }
            using (reader = new StreamReader(watchedListFilename))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    if (0 < line.Length)
                    {
                        if (IsPathFormatOK(line))
                        {
                            FolderList.Remove(line);
                            FolderList.Add(line, WatchType.Watched);
                        }
                        else
                        {
                            Debug.WriteLine("Discarded invalid path (" + line + ") from " + watchedListFilename + ".");
                        }
                    }
                }
            }
            //
            // Now that we've loaded all three files, write them back out to crystalize 
            // the normalization (i.e., the ellimination of conflicting entries).
            WriteWatchFiles();
        }

        #region GetterSetters
        /// <summary>
        /// Return a reference to the FolderList object.
        /// </summary>
        public Dictionary<string, WatchType> FolderList
        {
            get
            {
                return _FolderList;
            }
            set
            {
                _FolderList = value;
            }
        }
      
        /// <summary>
        /// Return true if the list is locked for writing.
        /// </summary>
        public bool WriteLocked
        {
            get
            {
                return _WriteLocked;
            }
            set
            {
                _WriteLocked = value;
            }
        }

        /// <summary>
        /// Return the list of Excluded folders as an array of strings.
        /// </summary>
        public string[] ExcludedFolders
        {
            get
            {
                List<string> theList = new();

                foreach (var item in FolderList)
                {
                    if (item.Value == WatchType.Excluded)
                    {
                        theList.Add(item.Key);
                    }
                }
                return theList.ToArray();
            }
        }

        /// <summary>
        /// Return the list of Onetime folders as an array of strings.
        /// </summary>
        public string[] OnetimeFolders
        {
            get
            {
                List<string> theList = new();

                foreach (var item in FolderList)
                {
                    if (item.Value == WatchType.Onetime)
                    {
                        theList.Add(item.Key);
                    }
                }
                return theList.ToArray();
            }
        }

        /// <summary>
        /// Return the list of Watched folders as an array of strings.
        /// </summary>
        public string[] WatchedFolders
        {
            get
            {
                List<string> theList = new();

                foreach (var item in FolderList)
                {
                    if (item.Value == WatchType.Watched)
                    {
                        theList.Add(item.Key);
                    }
                }
                return theList.ToArray();
            }
        }

        public object WatchedListsWriteLock
        {
            get => watchedListsWriteLock;
        }
        #endregion GetterSetters

        /// <summary>
        /// Write all the Watched lists to disk.
        /// </summary>
        public void WriteWatchFiles()
        {
            File.WriteAllLines(watchedListFilename, WatchedFolders);
            File.WriteAllLines(oneTimeListFilename, OnetimeFolders);
            File.WriteAllLines(excludeListFilename, ExcludedFolders);
        }

        /// <summary>
        /// Determine the WatchType of a folder.  In the absence of any specific override, a 
        /// folder inherits the WatchType of its parent.  So, the algorithm is as follows:
        ///  1. If the path = ThisPCStr (which is an artificial placeholder in the folder list)
        ///     then return Excluded.
        ///  2. If the path is explicitly mentioned as a Watched Lists folder, return its 
        ///     status.
        ///  3. If the path is the root of a drive, then return Excluded (since if it had
        ///     been Watched, it would have been found in the previous step).
        ///  4. Take the containing folder's name and see if it is explicitly mentioned 
        ///     as a Watched Lists folder.  If it is, then return its disposition, 
        ///     otherwise, repeat this step until the root folder it found.
        /// </summary>
        /// <param name="pathStr"></param>
        /// <returns></returns>
        public static WatchType FolderDisposition(string pathStr)
        {
#pragma warning disable CA1854 // Prefer the IDictionary.TryGetValue(TKey, out TValue) method
            if (pathStr.Equals(ThisPCStr, StringComparison.Ordinal))
            {
                return WatchType.Excluded;
            }
            else
            {
                if (Instance.FolderList.ContainsKey(pathStr))
                {
                    return Instance.FolderList[pathStr];
                }
                else
                {
                    if (pathStr.EndsWith(@":\", StringComparison.Ordinal))
                    {
                        return WatchType.Excluded;
                    }
                    else
                    {
                        string subPath = pathStr;

                        do
                        {
                            subPath = System.IO.Path.GetDirectoryName(subPath);

                            if (Instance.FolderList.ContainsKey(subPath))
                            {
                                return Instance.FolderList[subPath];
                            }
                        } while (!subPath.EndsWith(@":\", StringComparison.Ordinal));

                        return WatchType.Excluded;
                    }
                }
            }
#pragma warning restore CA1854 // Prefer the IDictionary.TryGetValue(TKey, out TValue) method
        }
    
        private static bool IsPathFormatOK(string pathStr)
        {
            bool pathOK;

            try 
            {
                pathOK = Path.IsPathFullyQualified(pathStr);    
            }
            catch 
            {
                pathOK = false;
            }
            return pathOK;
        }
    }
}
