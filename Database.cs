#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2026 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using LiteDB;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Micasa
{
    public class Database
    {
        /// <summary>
        /// The database is kept in the user's AppData folder.  Since each user has a unique 
        /// path to their AppData folder, the database path also needs to be calculated at 
        /// runtime.
        /// </summary>
        public static string DBFilename
        {
            get
            {
                string[] dbElements = [MainWindow.AppData, Constants.sMcAppDataFolder, Constants.sMcMicasaDBFileNm];
                return Path.Combine(dbElements);
            }
        }

        /// <summary>
        /// Construct the connection string to be used to open the Micasa database files.
        /// We want to ensure that the files are always opened in "shared" mode; so, we
        /// use this method to ensure all database files are opened consistently.
        /// </summary>
        /// <param name="fn">Database filename to include in the connection string.</param>
        /// <returns>The connection string to use in a LiteDatabase constructor.</returns>
        public static string ConnectionString(string fn)
        {
            if (fn.Contains('"'))
            {
                throw new ArgumentException("ERROR: unexpected error creating or opening database file ("
                                             + fn + ").\n\nUnable to continue.  Note: a database filename"
                                             + " may NOT contain a double quote character(s).");
            }

            return @"Filename=""" + fn + @"""; Connection=shared; auto-rebuild=true";
        }

        /// <summary>
        /// Create the Micasa database.
        /// 
        /// There is a known issue with LiteDB: LiteDB loses precision when serializes 
        /// DateTime objects (https://github.com/mbdavid/LiteDB/issues/1765).  Specifically,
        /// While LiteDB stores dates as the UTC milliseconds since the Unix epoch, C# 
        /// internally stores DateTime as the amount of 100-nanosecond ticks since the Unix 
        /// epoch. This additional precision is lost when serializing to BSON.
        /// 
        /// A workaround to this issue is to implement a custom serializer.  The GitHub issue 
        /// offers a sample:
        ///     var mapper = new BsonMapper();
        ///     
        ///     mapper.RegisterType<DateTime>(
        ///         value => value.ToString("o", CultureInfo.InvariantCulture),
        ///         bson => DateTime.ParseExact(bson, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        ///     mapper.RegisterType<DateTimeOffset>(
        ///         value => value.ToString("o", CultureInfo.InvariantCulture),
        ///         bson => DateTimeOffset.ParseExact(bson, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        ///         
        ///     using (var db = new LiteDatabase("instance.db", mapper))
        ///     {
        ///      ...
        ///     }
        /// 
        /// For the moment, I'm using a private DateTimeEquals() function that manually
        /// compares two DateTime objects down to millisecond precision and no further.
        /// </summary>
        public static void CreateDB()
        {
            using (var db = new LiteDatabase(ConnectionString(Database.DBFilename)))
            {
                var PhotoCol = db.GetCollection<PhotosTbl>("Photos");
                var FolderCol = db.GetCollection<FoldersTbl>("Folders");

                try
                {
                    PhotoCol.EnsureIndex(x => x.FQFilename);
                    FolderCol.EnsureIndex(x => x.Pathname);
                }
                catch (IOException)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Add a photo to the Photos table in the database.  
        /// 
        /// TO DO: this function must eventually:
        ///  * check the table for an existing entry, and update it if it's found;
        ///  * look for an existing .Micasa file and read meta-data from it to write into
        ///    the database;
        ///  * look inside the file itself for EXIF data to be used (and possibly updated).
        /// </summary>
        /// <param name="col">The database coldection we're updating.</param>
        /// <param name="f">The full filename (including path) of the photo to add to the database.</param>
        /// <param name="PicasaIniExists">Does a Picasa.ini exist in this photo's folder?.</param>
        /// <param name="DotPicasa">The IniFile object that corresonds to the .picasa file in this photo's folder.</param>
        /// <param name="DotMicasa">The IniFile object that corresonds to the .micasa file in this photo's folder.</param>
        public static void AddPhotoToDB(ILiteCollection<PhotosTbl> pCol,
                                         string f, bool PicasaIniExists,
                                         IniFile DotPicasa, IniFile DotMisasa)
        {
            FileInfo fileInfo = new FileInfo(f);
#pragma warning disable CA1416 // Validate platform compatibility
            FileSecurity fileSecurity = fileInfo.GetAccessControl();
            IdentityReference owner = fileSecurity.GetOwner(typeof(NTAccount));
#pragma warning restore CA1416 // Validate platform compatibility
            Metadata metadata = new(f);
            // Retrieve a CultureInfo object.
            CultureInfo invC = CultureInfo.InvariantCulture;
            PhotosTbl aPhoto = new()
            {
                Picture = Path.GetFileName(f),
                Caption = metadata.GetMetadataValue(Metadata.Tagnames.CaptionTagNm),
                FileType = Path.GetExtension(f).ToLower(invC),
                Pathname = Path.GetDirectoryName(f),
                FQFilename = f,
                FileSize = fileInfo.Length,
                CreatedDate = File.GetCreationTime(f),
                ModifiedDate = File.GetLastWriteTime(f),
#pragma warning disable CA1416 // Validate platform compatibility
                FileOwner = $"{owner.Value}",
#pragma warning restore CA1416 // Validate platform compatibility
                TitleCaption = metadata.GetMetadataValue(Metadata.Tagnames.CaptionTagNm),
                XDimension = metadata.GetMetadataValue(Metadata.Tagnames.PixelXDimensionNm),
                YDimension = metadata.GetMetadataValue(Metadata.Tagnames.PixelYDimensionNm),
                CameraMake = metadata.GetMetadataValue(Metadata.Tagnames.MakeNm),
                CameraModel = metadata.GetMetadataValue(Metadata.Tagnames.ModelNm),
                ImgCreationDate = metadata.GetMetadataValue(Metadata.Tagnames.DateTimeNm),
                ImgDigitisedDate = metadata.GetMetadataValue(Metadata.Tagnames.DateTimeDigitizedNm),
                Orientation = metadata.GetMetadataValue(Metadata.Tagnames.OrientationNm),
                Flash = metadata.GetMetadataValue(Metadata.Tagnames.FlashNm),
                LensMaker = metadata.GetMetadataValue(Metadata.Tagnames.LensMakerNm),
                LensModel = metadata.GetMetadataValue(Metadata.Tagnames.LensModelNm),
                FocalLength = metadata.GetMetadataValue(Metadata.Tagnames.FocalLengthNm),
                FocalLength35mm = metadata.GetMetadataValue(Metadata.Tagnames.FocalLengthIn35mmFilmNm),
                ExposureTime = metadata.GetMetadataValue(Metadata.Tagnames.ExposureTimeNm),
                Aperture = metadata.GetMetadataValue(Metadata.Tagnames.ApertureValueNm),
                FNumber = metadata.GetMetadataValue(Metadata.Tagnames.FNumberNm),
                Distance = metadata.GetMetadataValue(Metadata.Tagnames.SubjectDistanceNm),
                ISO = metadata.GetMetadataValue(Metadata.Tagnames.ISONm),
                WhiteBalance = metadata.GetMetadataValue(Metadata.Tagnames.WhiteBalanceNm),
                MeteringMode = metadata.GetMetadataValue(Metadata.Tagnames.MeteringModeNm),
                ExposureProgram = metadata.GetMetadataValue(Metadata.Tagnames.ExposureProgramNm),
                ColorSpace = metadata.GetMetadataValue(Metadata.Tagnames.ColorSpaceNm),
                XResolution = metadata.GetMetadataValue(Metadata.Tagnames.XResolutionNm),
                YResolution = metadata.GetMetadataValue(Metadata.Tagnames.YResolutionNm),
                ResolutionUnit = metadata.GetMetadataValue(Metadata.Tagnames.ResolutionUnitNm),
                Artist = metadata.GetMetadataValue(Metadata.Tagnames.ArtistNm),
                Copyright = metadata.GetMetadataValue(Metadata.Tagnames.CopyrightNm),
                ShutterSpeed = metadata.GetMetadataValue(Metadata.Tagnames.ShutterSpeedValueNm),
                ExposureBias = metadata.GetMetadataValue(Metadata.Tagnames.ExposureBiasValueNm),
                MakerNote = metadata.GetMetadataValue(Metadata.Tagnames.MakeNm),
                UserComment = metadata.GetMetadataValue(Metadata.Tagnames.UserCommentNm),
                GPSVersion = metadata.GetMetadataValue(Metadata.Tagnames.GPSVersionIDNm),
                Faces = [""],
                Albums = [""]
            };
            var results = pCol.FindOne(x => x.FQFilename.Equals(f, StringComparison.Ordinal));
            // Some code to use as an example in the event that I need the EXIF data.
            //var file = ImageFile.FromFile(f);
            //var caption = file.Properties.Get<ExifAscii>(ExifTag.PNGDescription);
            Debug.Print("FileSize: " + aPhoto.FileSize.ToString(invC));

            MainStatusBar.Instance.StatusBarMsg = Path.GetFileName(f);
            if (results == null)
            {
                pCol.Insert(aPhoto);
            }
            else
            {
                if (!Database.IsDateTimeEqual(aPhoto.ModifiedDate, results.ModifiedDate))
                {
                    results.ModifiedDate = aPhoto.ModifiedDate;
                    pCol.Update(results);
                }
            }
        }

        /// <summary>
        /// Add a folder to the Folders table in the database.
        /// </summary>
        /// <param name="watchedPath">The watched path that encloses the folder 'pathname'.</param>
        /// <param name="fCol">The DB folder collection.</param>
        /// <param name="pathname">The full pathname of the folder being added.</param>
        /// <param name="scanCompleted">Has the scan of the folder been completed?</param>
        public static void AddFolderToDB(string watchedPath, ILiteCollection<FoldersTbl> fCol, string pathname,
                                            bool scanCompleted)
        {
            FoldersTbl aFolder = new()
            {
                Pathname = pathname,
                ModifiedDate = File.GetLastWriteTime(pathname),
                LastScannedDate = DateTime.Now,
                WatchedParent = watchedPath,
                CompletedScan = scanCompleted
            };
            // Look in the DB to see if there is already a record for this folder.
            var results = fCol.FindOne(x => x.Pathname.Equals(pathname, StringComparison.Ordinal));

            if (results == null)
            {
                // No record was found, so insert the new folder.
                fCol.Insert(aFolder);

                // We also trigger an update to the MainWindow's FolderListBox.
                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    MainWindow.AddPathToTree(MainWindow.Instance.dbFoldersItem, pathname);
                });
            }
            else
            {
                results.ModifiedDate = aFolder.ModifiedDate;
                results.LastScannedDate = aFolder.LastScannedDate;
                results.CompletedScan = aFolder.CompletedScan;
                fCol.Update(results);
            }
        }

        /// <summary>
        /// Delete a photo from the DB.
        /// 
        /// TO DO: Based on an options setting, delete any associated data from the .micasa
        /// file or leave it in place.
        /// </summary>
        /// <param name="pCol">The DB table from which the photo is to be deleted.</param>
        /// <param name="f">The full pathname to the photo to be deleted.</param>
        public static void DeletePhotoFromDB(ILiteCollection<PhotosTbl> pCol,
                                             string f)
        {
            // Attempt to retrieve the photo's entry and then delete the entry if it was found.
            var results = pCol.FindOne(x => x.FQFilename.Equals(f, StringComparison.Ordinal));
            if (results != null)
            {
                int Id = results.Id;
                pCol.Delete(Id);
            }
        }

        /// <summary>
        /// Compare two DateTime objects.  This function is needed to work around
        /// the fact that LiteDB stores dates as UTC milliseconds since the Unix 
        /// epoch; which is less precision than the C# DateTime object contains.  
        /// So, this function limits its comparison to milliseconds; that is, it 
        /// doesn't compare the ticks.
        /// </summary>
        /// <param name="date1">A DateTime</param>
        /// <param name="date2">A DateTime</param>
        /// <returns>True if the timestamps are equal.</returns>
        public static bool IsDateTimeEqual(DateTime date1, DateTime date2)
        {
            if ((date1.Year == date2.Year) && (date1.Month == date2.Month)
                && (date1.Day == date2.Day) && (date1.Hour == date2.Hour)
                && (date1.Minute == date2.Minute) && (date1.Second == date2.Second)
                && (date1.Millisecond == date2.Millisecond))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Get the list of folders from the Folders table in the database.
        /// 
        /// TO DO: Replace this method per this comment:
        ///     This method is of limited use since it returns the full list of folders as an 
        ///     array of strings.  For Micasa's initial implementation, handling all of the 
        ///     folders in a single array is acceptable.  However, as the number of folders
        ///     increases and the number of photos in each folder increases, this method will
        ///     need to be replaced with a more sophisticated method that retrieves the folders
        ///     one at a time.
        /// </summary>
        /// <param name="Col">The collection (database) from which to retreive the list of folders.</param>
        /// <returns>The list of folders as an array of strings (string[]).</returns>
        public static string[] GetFolders(ILiteCollection<FoldersTbl> Col)
        {
            var results = Col.Query()
                .OrderBy(x => x.Pathname)
                .Select(x => x.Pathname)
                .ToArray();

            return results;
        }
    }

    /// <summary>
    /// This DB table is used to keep track of each folder that holds pictures contained
    /// in the database.  The modification date for each folder is tracked.  
    /// 
    /// The WatchedParent is tracked here to support the use case where a user removes
    /// a folder from the Watched list and so all of the entries in the Photos table 
    /// can be quickly identified and deleted.
    /// 
    /// A useful utility to provide on the Tools menu might be a one that validates that 
    /// there is a proper correspondance between the folders in this table and the folders 
    /// in the Photos table.
    /// </summary>
    /// <param name="Id">Unique ID number assigned by LiteDB.</param>
    /// <param name="Pathname">The folder's fully qualified name; i.e., not a relative path.</param>
    /// <param name="ModifiedDate">The folder's modification date (retrieved from the file sysetem).</param>
    /// <param name="LastScannedDate">The time and date when this DB record was updated.</param>
    /// <param name="WatchedParent">The watchlist folder to which this folder is a child.</param>
    /// <param name="CompletedScan">Assigned a value of <c>true</c> when every file in the folder has been scanned.</param>
    public class FoldersTbl
    {
        public int Id { get; set; }
        public string Pathname { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime LastScannedDate { get; set; }
        public string WatchedParent { get; set; }
        public bool CompletedScan { get; set; }
    }

    /// <summary>
    /// This DB table is used to track all photos contained in Micasa Watched or One Tine 
    /// folder.
    ///
    /// The metadata for each photo is tracked in this table, and it is updated in
    /// two circumstances: 1) when the photo scanner detects that the modification date of the
    /// photo is newer than the date in the database; and, 2) when the user selects a photo in
    /// the viewer and the viewer detects that the modification date of the photo is newer
    /// than the date in the database.
    /// 
    /// EXIF data is typically stored as strings, and so the metadata is stored in the database
    /// as strings.  Where a string needs to be parsed into human readable form, helping methods
    /// in the Metadata class will be used to do the parsing.
    /// </summary>
    /// <remarks>
    /// TODO: Thumbnails -- both the embedded thumbnail and the thumbnail generated by Micasa -- 
    /// still need to be dealt with.
    /// </remarks>
    /// <param name="Id">Unique ID number assigned by LiteDB.</param>
    /// <param name="Picture">The filename of the picture file (including suffix).</param>
    /// <param name="Caption">The caption of the photo.</param> 
    /// <param name="FileType">The type of picture file (JPG, PNG, etc.); generally, the filename suffix.</param> 
    /// <param name="FQFilename">The fully qualified filename of the picture file; the full path and filename.</param> 
    /// <param name="Pathname">The full pathname; where the picture file is stored.</param> 
    /// <param name="FileSize">The file's size in megabytes.</param>
    /// <param name="CreatedDate">The file's creation date.</param>
    /// <param name="ModifiedDate">The file's modification date.</param> 
    /// <param name="FileOwner">The file's owner (this is not the logical owner of the photo, which is the Artist field).</param>
    /// <param name="TitleCaption">The photo's caption; either embedded in the file or in a local .micasa file.</param>
    /// <param name="XDimension">The photo's X dimension.</param>
    /// <param name="YDimension">The photo's Y dimension.</param>
    /// <param name="CameraMake">The make of camera used to take the photo.</param>
    /// <param name="CameraModel">The model of camera used to take the photo.</param>
    /// <param name="ImgCreationDate">The photo's embedded creation date.</param>
    /// <param name="ImgDigitisedDate">The photo's </param>
    /// <param name="Orientation">The photo's orientation (protrait or landscape).</param>
    /// <param name="Flash">The status of the flash when the photo was taken.</param>
    /// <param name="LensMaker">The brand of lens used to take the photo.</param>
    /// <param name="LensModel">the model of lens used to take the photo.</param>
    /// <param name="FocalLength">The focal length of the lens used to take the photo.</param>
    /// <param name="FocalLength35mm">The focal length of the lens used to take the photo in a 35mm context.</param>
    /// <param name="ExposureTime">The exposure time of the photo.</param>
    /// <param name="Aperture">The aperture used to take the photo.</param>
    /// <param name="FNumber">The f-stop used to take the photo.</param>
    /// <param name="Distance">The distance reported by the lens. </param>
    /// <param name="ISO">The camera's ISO setting used for the photo. </param>
    /// <param name="WhiteBalance">The camera's white balance for the photo.</param>
    /// <param name="MeteringMode">The camera's metering mode for the photo.</param>
    /// <param name="ExposureProgram">The camera's exposure program for the photo.</param>
    /// <param name="ColorSpace">The camera's color space for the photo. </param>
    /// <param name="JPEGQuality">The quality setting of the photo's JPEG image.</param>
    /// <param name="UniqueID">The photo's unique identifier.</param>
    /// <param name="XResolution">The photo's X resolution.</param>
    /// <param name="YResolution">The photo's Y resolution.</param>
    /// <param name="ResolutionUnit">The units of resolution.</param>
    /// <param name="Software">The software used to process the photo.</param>
    /// <param name="Artist">The name of the individual who took the photo.</param>
    /// <param name="Copyright">The copyright status of the photo.</param>
    /// <param name="ShutterSpeed">The shutter speed of the photo.</param>
    /// <param name="ExposureBias">the exposure bias of the photo.</param>
    /// <param name="MakerNote">Any note added to the photo by the photographer.</param>
    /// <param name="UserComment">Any user comment added to the photo by the photographer.</param>
    /// <param name="GPSVersion">The photo's GPS version.</param>
    /// <param name="EXIFVersion">The photo's EXIF version.</param>
    /// <param name="Faces">A JSON object containing the faces in the photo and their locations.</param> 
    /// <param name="Albums">The list of albums in which this photo is included (JSON object).</param> 
    public class PhotosTbl
    {
        public int Id { get; set; }
        public string Picture { get; set; }
        public string Caption { get; set; }
        public string FileType { get; set; }
        public string FQFilename { get; set; }
        public string Pathname { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string FileOwner { get; set; }
        public string TitleCaption { get; set; }
        public string XDimension { get; set; }
        public string YDimension { get; set; }
        public string CameraMake { get; set; }
        public string CameraModel { get; set; }
        public string ImgCreationDate { get; set; }
        public string ImgDigitisedDate { get; set; }
        public string Orientation { get; set; }
        public string Flash { get; set; }
        public string LensMaker { get; set; }
        public string LensModel { get; set; }
        public string FocalLength { get; set; }
        public string FocalLength35mm { get; set; }
        public string ExposureTime { get; set; }
        public string Aperture { get; set; }
        public string FNumber { get; set; }
        public string Distance { get; set; }
        public string ISO { get; set; }
        public string WhiteBalance { get; set; }
        public string MeteringMode { get; set; }
        public string ExposureProgram { get; set; }
        public string ColorSpace { get; set; }
        public string JPEGQuality { get; set; }
        public string UniqueID { get; set; }
        public string XResolution { get; set; }
        public string YResolution { get; set; }
        public string ResolutionUnit { get; set; }
        public string Software { get; set; }
        public string Artist { get; set; }
        public string Copyright { get; set; }
        public string ShutterSpeed { get; set; }
        public string ExposureBias { get; set; }
        public string MakerNote { get; set; }
        public string UserComment { get; set; }
        public string GPSVersion { get; set; }
        public string EXIFVersion { get; set; }
        public string[] Faces { get; set; }
        public string[] Albums { get; set; }
    }
}
