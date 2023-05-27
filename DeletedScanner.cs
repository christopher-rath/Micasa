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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Micasa
{
    class DeletedScanner
    {
        /// <summary>
        /// In parallel with a scan for new pictures, we also need to scan for 
        /// deleted pictures.  We don't do that scan from the Watch list 
        /// because we're trying to find database entries that no longer link
        /// to an object in the filesystem.
        /// 
        ///  1. For each folder listed in the Folders table (retrieve the records
        ///     sorted by modification date, newest first—folders recently
        ///     modified are more likely to have activity):
        ///     a. If the folder does NOT exist then delete all entries in the
        ///        Photos table where the entry had been stored in this folder.
        ///     b. If the folder does exist AND the folder’s modification date
        ///        is greater than the value stored in the DB for the folder
        ///        (i.e., the contents of the folder have been modified--a file
        ///        has been added or deleted--since we last scanned the folder):
        ///           i. Use the folder name to retrieve records from the Photos
        ///              table, and for each record retrieved:
        ///               1. If the file does not exist then move any .Micasa
        ///                  entries for the record into a .Micasa file in the
        ///                  .MicasaOriginals folder; and,
        ///               2. Delete the record from the Photos table.
        ///          ii. Check the Photos table and if there are no records left
        ///              for this folder (that is, the condition that occurs when
        ///              all files in a folder have been deleted) then delete the
        ///              folder from Folders table.
        ///         iii. If there are one or more records in the Photos table for
        ///              this folder then:
        ///               1. Re-retrieve the folder’s modification date in case it
        ///                  has changed;
        ///               2. Update the modification date for this folder in the
        ///                  Folders table;
        ///               3. Update the last deletion scan date for this folder in
        ///                  Folders table.
        /// </summary>
        /// <param name="token"></param>
        public static void StartScanner(object token)
        {
            CancellationToken myCancelToken = (CancellationToken)token;

            // Pause for 10 seconds to allow the picture scanner time to get itself fully 
            // up and running.
            Thread.Sleep(10000);
            using (var db = new LiteDatabase(Database.ConnectionString(Database.DBFilename)))
            {
                ILiteCollection<PhotosTbl> PhotoCol = db.GetCollection<PhotosTbl>(Constants.sMcPhotosColNm);
                ILiteCollection<FoldersTbl> FolderCol = db.GetCollection<FoldersTbl>(Constants.sMcFoldersColNm);

                if (!myCancelToken.IsCancellationRequested)
	        {
		    // The actions to be taken.  Until there is work to be done this
		    // scanner simply falls through and stops running.
		}
            }
        }
    }
}
