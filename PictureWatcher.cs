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
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading;
using System.Windows.Shapes;

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
                WatcherChangeTypes pAction,
                string fullpath,
                string filename,
                string oldpath,
                string oldname)
            {
                PAction = pAction;
                Fullpath = fullpath;
                Filename = filename;
                Oldpath = oldpath;
                Oldname = oldname;
            }
            public WatcherChangeTypes PAction { get; private set; }
            public string Fullpath { get; private set; }
            public string Filename { get; private set; }
            public string Oldpath { get; private set; }
            public string Oldname { get; private set; }
        }
        private static readonly Queue<PhotoToQueue> _photosToProcess = new();

        #region GetterSetters
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
        /// Start the photo processor that takes filenames from the PictureWatcher List<T>
        /// and processes the makes the applicable updates to the Micasa photo database.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        public static void StartProcessor(object token)
        {
            CancellationToken myCancelToken = (CancellationToken)token;

            // Pause for a moment to provide a delay to make threads more apparent.
            Thread.Sleep(500);

            // Open the database.
            using (var db = new LiteDatabase(Database.ConnectionString(Database.DBFilename)))
            {
                ILiteCollection<PhotosTbl> PhotoCol = db.GetCollection<PhotosTbl>(Constants.sMcPhotosColNm);
                ILiteCollection<FoldersTbl> FolderCol = db.GetCollection<FoldersTbl>(Constants.sMcFoldersColNm);

                // Loop until asked to cancel.
                while (!myCancelToken.IsCancellationRequested)
                {
                    bool firstLoop = true;
                    PhotoToQueue ptq;
                    PhotoToQueue ptqPk;

                    // Process photos in the queue until its empty or a request to cancel
                    // has been made.
                    if (_photosToProcess.Count > 0)
                    {
                        firstLoop = true;

                        // Get the first entry and then start to loop if there are more entries.
                        ptq = _photosToProcess.Dequeue();

                        // Sleep briefly to allow multiple events to post.
                        Thread.Sleep(50);
                        // If there are multiple events in the Queue<T> then loop; otherwise just
                        // process the single event.
                        if (_photosToProcess.Count == 0)
                        {
                            ProcessFileChangeEvent(ptq);
                            Debug.WriteLine($"Processed (no loop): {ptq.Fullpath}");
                        }
                        else
                        {
                            while ((_photosToProcess.Count > 0) && (!myCancelToken.IsCancellationRequested))
                            {
                                if (!firstLoop && (_photosToProcess.Count > 0))
                                {
                                    ptq = _photosToProcess.Dequeue();
                                }
                                else
                                {
                                    firstLoop = false;
                                }

                                if (_photosToProcess.Count > 0)
                                {
                                    // There is a dequeued entry and another one still in the queue; so, we
                                    // check for the opportunity to collapse the events.
                                    ptqPk = _photosToProcess.Peek();
                                    if (((ptq.PAction == WatcherChangeTypes.Created) || (ptq.PAction == WatcherChangeTypes.Changed))
                                        && ((ptqPk.PAction == WatcherChangeTypes.Created) || (ptqPk.PAction == WatcherChangeTypes.Changed))
                                        && (ptq.Fullpath == ptqPk.Fullpath))
                                    {
                                        // The two events are for the same Fullpath and are either Created or Changed
                                        // events; so, we are allowed to collapse them into a single event.  We will
                                        // dequeue the next entry overtop of the previous one at the top of the while()
                                        // loop.
                                        Debug.WriteLine($"Collapsed: {ptq.Fullpath}");
                                        MainStatusBar.Instance.StatusBarMsg = ptq.Filename;
                                    }
                                    else
                                    {
                                        // We are not allowed to collapse the two events; so, we process the one that
                                        // was last dequeued.
                                        Debug.WriteLine($"Processed (tuple): {ptq.Fullpath}");
                                        ProcessFileChangeEvent(ptq);
                                    }
                                }
                                else
                                {
                                    // There was only one entry in the Queue<T>; so, we simply it.
                                    Debug.WriteLine($"Processed (single): {ptq.Fullpath}");
                                    ProcessFileChangeEvent(ptq);
                                }
                            }
                        }
                    }
                }
            }
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
            if (Options.Instance.IsFileTypeToScan(e.FullPath)
                && WatchedLists.FolderDisposition(e.FullPath) == WatchType.Watched)
            {
                PhotoToQueue p = new(e.ChangeType, e.FullPath, e.Name, "", "");
                _photosToProcess.Enqueue(p);
                Debug.WriteLine($"Changed: {e.FullPath}");
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (Options.Instance.IsFileTypeToScan(e.FullPath)
                && WatchedLists.FolderDisposition(e.FullPath) == WatchType.Watched)
            {
                PhotoToQueue p = new(e.ChangeType, e.FullPath, e.Name, "", "");
                _photosToProcess.Enqueue(p);
                Debug.WriteLine($"Created: {e.FullPath}");
            }
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Options.Instance.IsFileTypeToScan(e.FullPath)
                && WatchedLists.FolderDisposition(e.FullPath) == WatchType.Watched)
            {
                PhotoToQueue p = new(e.ChangeType, e.FullPath, e.Name, "", "");
                _photosToProcess.Enqueue(p);
                Debug.WriteLine($"Deleted: {e.FullPath}");
            }
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            PhotoToQueue p = new(e.ChangeType, e.FullPath, e.Name, e.OldFullPath, e.OldName);
            _photosToProcess.Enqueue(p);
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

        /// <summary>
        /// Process a file change event that has been dequeued from Queue<PhotoToQueue>.
        /// </summary>
        /// <param name="ptq">The dequeued event to process.</param>
        private static void ProcessFileChangeEvent(PhotoToQueue ptq)
        {
            MainStatusBar.Instance.StatusBarMsg = ptq.Filename;
            if (ptq.PAction == WatcherChangeTypes.Created || ptq.PAction == WatcherChangeTypes.Changed)
            {
                // Process these as an adding a photo.  The add methods handle updates to existing photos.
                //Database.AddPhotoToDB(PhotoCol, ptq.Fullpath, PicasaIniExists, DotPicasa, DotMicasa);
                //Database.AddFolderToDB(wPath, FolderCol, Path.GetDirectoryName(ptq.Fullpath), false);
                // TO DO: add thumbnail
            }
            else if (ptq.PAction == WatcherChangeTypes.Renamed)
            {
                // Process the renaming of a photo.
            } 
            else
            { 
                // Process the deletion of a photo.
            }
        }
    }
}
