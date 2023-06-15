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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Micasa
{
    /// <summary>
    /// Use FileSystemWatcher to monitor folders that are on the WatchedFolders 
    /// list.  Do ensure that changes to the WatchedFolders list are reflected
    /// in the set of FileSystemWatchers, the Folder Manager panel needs to stop
    /// the watchers and restart them; just as it does with the filesystem 
    /// scanners.
    /// </summary>
    internal sealed class PictureWatcher
    {
        // Each folder to be monitored has an active FileSystemWatcher.  We maintain
        // the set in this list.
        private readonly List<FileSystemWatcher> _activeWatchers = new();
        public struct PhotoToQueue
        {
            public PhotoToQueue(
                Int64 eventTicks,
                WatcherChangeTypes pAction,
                string fullpath,
                string filename,
                string oldpath,
                string oldname)
            {
                EventTicks = eventTicks;
                PAction = pAction;
                Fullpath = fullpath;
                Filename = filename;
                Oldpath = oldpath;
                Oldname = oldname;
            }
            public Int64 EventTicks { get; private set; }
            public WatcherChangeTypes PAction { get; private set; }
            public string Fullpath { get; private set; }
            public string Filename { get; private set; }
            public string Oldpath { get; private set; }
            public string Oldname { get; private set; }
        }
        private static readonly Queue<PhotoToQueue> _photosToProcess = new();

        #region GetterSetters
        public static int QCount => _photosToProcess.Count;

        public static PhotoToQueue QDequeue => _photosToProcess.Dequeue();

        public static PhotoToQueue QPeek => _photosToProcess.Peek();
        #endregion GetterSetters

        /// <summary>
        /// Begin to watch a new folder for new, changed, renamed, and deleted
        /// photos.  The FileSystemWatcher always watches recursively; so, it's
        /// up to the code that handles notifications to ignore changes that 
        /// occur in folders that are to be ignored.
        /// </summary>
        /// <param name="wPath">The folder to monitor.</param>
        public void WatchFolder(string wPath)
        {
            FileSystemWatcher watcher = new()
            {
                Path = wPath,
                NotifyFilter = NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastWrite,
                IncludeSubdirectories = true
            };
            foreach (string type in Options.Instance.PhotoTypesToWatch)
            {
                watcher.Filters.Add(type);
            }
            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;
            watcher.EnableRaisingEvents = true;
            _activeWatchers.Add(watcher);
            Debug.WriteLine($"PictureWather: started watching {wPath}");
        }

        /// <summary>
        /// Stop all the watchers by disposing the objects and then clearing the list.
        /// </summary>
        public void StopWatchers()
        {
            foreach (FileSystemWatcher fsw in _activeWatchers)
            {
                fsw.Dispose();
            }
            _activeWatchers.Clear();
            Debug.WriteLine("PictureWatcher: stopped watchers.");
        }

        public void Dispose()
        {
            StopWatchers();
            this.Dispose();
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            QueuePhoto(e.ChangeType, e.FullPath, e.Name, "", "");
            Debug.WriteLine($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            QueuePhoto(e.ChangeType, e.FullPath, e.Name, "", "");
            Debug.WriteLine($"Created: {e.FullPath}");
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            QueuePhoto(e.ChangeType, e.FullPath, e.Name, "", "");
            Debug.WriteLine($"Deleted: {e.FullPath}");
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            QueuePhoto(e.ChangeType, e.FullPath, e.Name, e.OldFullPath, e.OldName);
            Debug.WriteLine(@"Renamed:");
            Debug.WriteLine($"    Old: {e.OldFullPath}");
            Debug.WriteLine($"    New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine("Stacktrace:");
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine($"");
                PrintException(ex.InnerException);
            }
        }

        private static void QueuePhoto(WatcherChangeTypes c, string path, string name, string oldpath, string oldname)
        {
            PhotoToQueue p = new(DateTime.Now.Ticks, c, path, name, oldpath, oldname);
            _photosToProcess.Enqueue(p);
            Debug.WriteLine($"Queued: {path}");
        }
    }
}
