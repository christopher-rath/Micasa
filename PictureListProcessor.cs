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
using System.Threading.Tasks;
using static Micasa.PictureWatcher;

namespace Micasa
{
    internal sealed class PictureListProcessor
    {
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

                // Loop until asekd to cancel; however, the thread will sleep between iterations
                // of this while() loop.
                while (!myCancelToken.IsCancellationRequested)
                {
                    bool firstLoop = true;
                    PhotoToQueue ptq;
                    PhotoToQueue ptqPk;

                    // Process photos in the queue until its empty or a request to cancel
                    // has been made.
                    if (PictureWatcher.QCount > 0)
                    {
                        firstLoop = true;

                        // Get the first entry and then start to loop if there are more entries.
                        ptq = PictureWatcher.QDequeue;

                        // Sleep briefly to allow multiple events to post.
                        Thread.Sleep(50);
                        // If there are mulitple events in the Queue<T> then loop; otherwise just
                        // process the single event.
                        if (PictureWatcher.QCount == 0)
                        {
                            Debug.WriteLine($"Processed (no loop): {ptq.Fullpath}");
                        } 
                        else
                        {
                            while ((PictureWatcher.QCount > 0) && (!myCancelToken.IsCancellationRequested))
                            {
                                if (!firstLoop && (PictureWatcher.QCount > 0))
                                {
                                    ptq = PictureWatcher.QDequeue;
                                }
                                else
                                {
                                    firstLoop = false;
                                }

                                if (PictureWatcher.QCount > 0)
                                {
                                    // There is a dequeued entry and another one still in the queue; so, we
                                    // check for the opportunity to collapse the events.
                                    ptqPk = PictureWatcher.QPeek;
                                    if (((ptq.PAction == WatcherChangeTypes.Created) || (ptq.PAction == WatcherChangeTypes.Changed))
                                        && ((ptqPk.PAction == WatcherChangeTypes.Created) || (ptqPk.PAction == WatcherChangeTypes.Changed))
                                        && (ptq.Fullpath == ptqPk.Fullpath))
                                    {
                                        // The two events are for the same Fullpath and are either Created or Changed
                                        // events; so, we are allowed to collapse them into a single event.  We 
                                        // dequeue the next entry overtop of the previous one.
                                        //ptq = PictureWatcher.QDequeue;
                                        Debug.WriteLine($"Collapsed: {ptq.Fullpath}");
                                    }
                                    else
                                    {
                                        // We are not allowed to collapse the two events; so, we process the one that
                                        // was last dequeued.
                                        Debug.WriteLine($"Processed (tuple): {ptq.Fullpath}");
                                    }
                                }
                                else
                                {
                                    // There was only one entry in the Queue<T>; so, we simply it.
                                    Debug.WriteLine($"Processed (single): {ptq.Fullpath}");
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
