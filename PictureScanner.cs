
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using LiteDB;
using System.Diagnostics;
using ExifLibrary;
using System.Windows.Media.Imaging;
using System.Globalization;

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
                                        AddPhotoToDB(PhotoCol, f, PicasaIniExists, DotPicasa, DotMicasa);
                                        AddFolderToDB(wPath, FolderCol, Path.GetDirectoryName(f), false);
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
                    AddFolderToDB(wPath, FolderCol, wPath, true);
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

                    Debug.WriteLine(d + " ==> " + WatchedLists.FolderDisposition(d));
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
                                    AddPhotoToDB(pCol, f, PicasaIniExists, DotPicasa, DotMicasa);
                                    AddFolderToDB(wDir, fCol, Path.GetDirectoryName(f), false);
                                }
                            }
                        }
                        /// Now that we've scanned all files and all folders (recursively) in folder 'd', we
                        /// mark scanCompmleted as true.
                        AddFolderToDB(wDir, fCol, d, true);
                    }
                    Scanfolder(pCol, fCol, d, wDir, myCancelToken);
                }
            }
        }

        /// <summary>
        /// Add a photo to the Photos table in the database.  This function must eventually:
        ///  * check the table for an existing entry, and update it if it's found;
        ///  * look for an existing .Micasa file and read meta-data from it to write into
        ///    the database;
        ///  * look inside the file itself for EXIF data to be used (and possibly updated).
        /// </summary>
        /// <param name="col">The database coldection we're updating.</param>
        /// <param name="f">The filename of the photo to add to the database.</param>
        /// <param name="dir">The folder in which the filename is located.</param>
        private static void AddPhotoToDB(ILiteCollection<PhotosTbl> pCol, 
                                         string f, bool PicasaIniExists,
                                         IniFile DotPicasa, IniFile DotMisasa)
        {
            // Retrieve a CultureInfo object.
            CultureInfo invC = CultureInfo.InvariantCulture;
            PhotosTbl aPhoto = new()
            {
                Picture = Path.GetFileName(f),
                Caption = GetCaptionFromImage(f),
                FileType = Path.GetExtension(f).ToLower(invC),
                Pathname = Path.GetDirectoryName(f),
                FQFilename = f,
                ModificationDate = File.GetLastWriteTime(f),
                Faces = new string[] { "" },
                Albums = new string[] { "" }
            };
            var results = pCol.FindOne(x => x.FQFilename.Equals(f, StringComparison.Ordinal));
            // Some code to use as an example in the event that I need the EXIF data.
            //var file = ImageFile.FromFile(f);
            //var caption = file.Properties.Get<ExifAscii>(ExifTag.PNGDescription);

            if (results == null)
            {
                pCol.Insert(aPhoto);
            }
            else
            {
                if (!PhotosTbl.IsDateTimeEqual(aPhoto.ModificationDate, results.ModificationDate))
                {
                    results.ModificationDate = aPhoto.ModificationDate;
                    pCol.Update(results);
                }
            }
        }

        private static void AddFolderToDB(string watchedPath, ILiteCollection<FoldersTbl> fCol, string pathname,
                                            bool scanCompleted)
        {
            FoldersTbl aFolder = new()
            {
                Pathname = pathname,
                ModificationDate = File.GetLastWriteTime(pathname),
                LastScannedDate = DateTime.Now,
                WatchedParent = watchedPath,
                CompletedScan = scanCompleted
            };
            var results = fCol.FindOne(x => x.Pathname.Equals(pathname, StringComparison.Ordinal));

            if (results == null)
            {
                fCol.Insert(aFolder);
            }
            else
            {
                results.ModificationDate = aFolder.ModificationDate;
                results.LastScannedDate = aFolder.LastScannedDate;
                results.CompletedScan = aFolder.CompletedScan;
                fCol.Update(results);
            }
        }


        /// <summary>
        /// Get the Caption from the image file.  
        /// 
        /// If any error occurs, this method will silently return an empty string.
        /// </summary>
        /// <param name="imgFl">Filename with any required path.</param>
        /// <returns>A string.</returns>
        private static string GetCaptionFromImage(string imgFl)
        {
            string caption = "";
            // Retrieve a CultureInfo object.
            CultureInfo invC = CultureInfo.InvariantCulture;
            bool supportedImg = false;

            switch (Path.GetExtension(imgFl).ToLower(invC))
            {
                case Constants.sMcFT_Jpg:
                case Constants.sMcFT_JpgA:
                case Constants.sMcFT_Tif:
                case Constants.sMcFT_TifA:
                    supportedImg = true;
                    break;
                default:
                    supportedImg = false;
                    break;
            }
            if (supportedImg)
            {
                try
                {
                    using (FileStream fs = File.OpenRead(imgFl))
                    {
                        BitmapSource img = BitmapFrame.Create(fs);
                        BitmapMetadata md = (BitmapMetadata)img.Metadata;

                        caption = md.Title;
                        if (caption == null)
                        {
                            caption = "";
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine(string.Format(invC, "GetCaptionFromImage ({0}): Unknown exception; returning empty string.", imgFl));
                }
            }
            return caption;
        }
    }
}
