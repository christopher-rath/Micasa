#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2024 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System.Threading;
using System.IO;
using LiteDB;
using System.Diagnostics;

namespace Micasa
{
    sealed class PictureScanner
    {
        /// <summary>
        /// When files are added to a folder, or an existing file is modified,
        /// there is no reliable way to determine from its containing folder that
        /// something has been changed in that folder.  As a result, the entire
        /// hierarchy below each Watched folder must be scanned.  
        /// 
        /// A potential way to expedite such a search may be to through use of the
        /// Folders table and examination of the modfication dates, and then
        /// scanning folders most recently modified (the thinking being that
        /// collections of photos in folders are modified for a period of time and
        /// then sit static thereafter).
        ///
        /// So, for each folder in the Watch list begin a recursive scan, and for
        /// each file found (FQ-Filename is the fully qualified filename):
        ///  1. if AppMode = Legacy
        ///     a. take the FQ-Filename
        ///     b. look in the database for an entry, and if there’s an entry,
        ///        compare the DB modified date with the FQ-Filename modified date
        ///         i. if the FQ-Filename is newer then take its metadata as master
        ///        ii. if the database is newer, discard any FQ-Filename values
        ///            that are already stored in the database
        ///     c. load the .picasa file, use it’s information to supplement the
        ///        data thus far (but don’t over-ride any already populated
        ///        fields)
        ///     d. write the data to the database
        ///     e. write the data to the .picasa file
        ///     f. if the filetype is .JPG then write the data to the FQ-Filename
        ///  2. if AppMode = Native
        ///     a. take the FQ-Filename
        ///     b. look in the database for an entry, and if there’s an entry,
        ///        compare the DB modified date with the FQ-Filename modified date
        ///         i. if the FQ-Filename is newer then take its metadata as master
        ///        ii. if the database is newer, discard any FQ-Filename values that 
        ///            are already stored in the database
        ///     c. load the .Micasa file
        ///         i. if the database modification date is newer than the .Micasa
        ///            entry’s modification date, use the .Micasa information to
        ///            supplement the data thus far (but don’t over-ride any
        ///            already populated fields)
        ///        ii. else, use the .Micasa information as the master metadata
        ///     d. write the data to the database
        ///          i. write the record to the Photos table
        ///         ii. write the record to the Folders table (or update an existing
        ///             record)
        ///     e. write the data to the .Micasa file
        ///     f. if UpdatePhotoFiles = true then write the data to the FQ-Filename
        ///  3. if AppMode = Migrate
        ///     a. take the FQ-Filename
        ///     b. look in the database for an entry, and if there’s an entry,
        ///        compare the DB modified date with the FQ-Filename modified date
        ///         i. if the FQ-Filename is newer then take its metadata as master
        ///        ii. if the database is newer, discard any FQ-Filename values
        ///            that are already stored in the database
        ///     c. load the .Micasa file
        ///         i. if the database modification date is newer than the .Micasa
        ///            entry’s modification date, use the .Micasa information to
        ///            supplement the data thus far (but don’t over-ride any
        ///            already populated fields)
        ///        ii. else, use the .Micasa information as the master metadata
        ///     d. load the .picasa file, use it’s information to supplement the
        ///        data thus far (but don’t over-ride any already populated
        ///        fields)
        ///     e. write the data to the database
        ///     f. write the data to the .Micasa file
        ///     g. remove data for this FQ-Filename from the .picasa file
        ///     h. if UpdatePhotoFiles = true then write the data to the FQ-Filename
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        public static void StartScanner(object token)
        {
            CancellationToken myCancelToken = (CancellationToken)token;

            // Pause for a moment to provide a delay to make threads more apparent.
            Thread.Sleep(500);
            using (var db = new LiteDatabase(Database.ConnectionString(Database.DBFilename)))
            {
                ILiteCollection<PhotosTbl> PhotoCol = db.GetCollection<PhotosTbl>(Constants.sMcPhotosColNm);
                ILiteCollection<FoldersTbl> FolderCol = db.GetCollection<FoldersTbl>(Constants.sMcFoldersColNm);

                // For each folder in the watchlist we have to 
                //  a) retrive the files in that folder and add the appropriate ones
                //     to the database.
                //  b) retrieve the folders in this folder and for each folder
                //     that has a status of Watched then step into that folder and
                //     process its files and folders.
                foreach (string wPath in WatchedLists.Instance.WatchedFolders)
                {
                    try
                    {
                        // Create IniFile objects for the .Micasa and .Picasa files in this folder.
                        bool PicasaIniExists = File.Exists(wPath + Path.DirectorySeparatorChar + Constants.sMcDotPicasa);
                        IniFile DotPicasa = new(wPath + Path.DirectorySeparatorChar + Constants.sMcDotPicasa);
                        IniFile DotMicasa = new(wPath + Path.DirectorySeparatorChar + Constants.sMcDotMicasa);

                        if (!myCancelToken.IsCancellationRequested)
                        {
                            foreach (string f in Directory.GetFiles(wPath))
                            {
                                if (myCancelToken.IsCancellationRequested)
                                {
                                    // Give the database a second to finish flushing to disk.
                                    Thread.Sleep(1000);
                                    break;
                                }
                                else
                                {
                                    if (Options.Instance.IsFileTypeToScan(f))
                                    {
                                        Database.AddPhotoToDB(PhotoCol, f, PicasaIniExists, DotPicasa, DotMicasa);
                                        Database.AddFolderToDB(wPath, FolderCol, Path.GetDirectoryName(f), false);
                                        // TO DO: add thumbnail
                                    }
                                }
                            }
                            Scanfolder(PhotoCol, FolderCol, wPath, wPath, myCancelToken);
                        }
                    }
                    catch
                    {
                        break;
                    }
                    /// Now that we've scanned all files and all folders (recursively) in this Watched folder, we
                    /// mark scanCompmleted as true.
                    Database.AddFolderToDB(wPath, FolderCol, wPath, true);
                }
            }
        }

        /// <summary>
        /// Recursively scan a folder structure and add any qualifying photos to the
        /// database.
        /// </summary>
        /// <param name="pCol">A Photos database table</param>
        /// <param name="fCol">A Folders database table</param>
        /// <param name="dir">The folder to be scanned</param>
        /// <param name="wDir">The parent Watched folder</param>
        /// <param name="myCancelToken">The thread's CancellationToken</param>
        private static void Scanfolder(ILiteCollection<PhotosTbl> pCol, ILiteCollection<FoldersTbl> fCol, string dir, 
                                       string wDir, CancellationToken myCancelToken)
        {
            foreach (string d in Directory.GetDirectories(dir))
            {
                if (!myCancelToken.IsCancellationRequested)
                {
                    // Create IniFile objects for the .Micasa and .Picasa files in this folder.
                    bool PicasaIniExists = File.Exists(d + Path.DirectorySeparatorChar + Constants.sMcDotPicasa);
                    IniFile DotPicasa = new(d + Path.DirectorySeparatorChar + Constants.sMcDotPicasa);
                    IniFile DotMicasa = new(d + Path.DirectorySeparatorChar + Constants.sMcDotMicasa);

                    Debug.WriteLine("Folder disposition: " + d + " ==> " + WatchedLists.FolderDisposition(d));
                    if (WatchedLists.FolderDisposition(d) == WatchType.Watched)
                    {
                        foreach (string f in Directory.GetFiles(d))
                        {
                            if (myCancelToken.IsCancellationRequested)
                            {
                                // Give the database a second to finish flushing to disk.
                                Thread.Sleep(1000);
                                break;
                            }
                            else
                            {
                                if (Options.Instance.IsFileTypeToScan(f))
                                {
                                    Database.AddPhotoToDB(pCol, f, PicasaIniExists, DotPicasa, DotMicasa);
                                    Database.AddFolderToDB(wDir, fCol, Path.GetDirectoryName(f), false);
                                }
                            }
                        }
                        /// Now that we've scanned all files and all folders (recursively) in folder 'd', we
                        /// mark scanCompmleted as true.
                        Database.AddFolderToDB(wDir, fCol, d, true);
                    }
                    Scanfolder(pCol, fCol, d, wDir, myCancelToken);
                }
            }
        }
    }
}
