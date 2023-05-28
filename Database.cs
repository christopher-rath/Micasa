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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LiteDB;

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
                string[] dbElements = { MainWindow.AppData, Constants.sMcAppDataFolder, Constants.sMcMicasaDBFileNm };
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

            return @"Filename=""" + fn + @"""; Connection=shared";
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
        //      using (var db = new LiteDatabase("instance.db", mapper))
        //      {
        //       ...
        //      }
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
    public class FoldersTbl
    {
        public int Id { get; set; }
        public string Pathname { get; set; }
        public DateTime ModificationDate { get; set; }
        public DateTime LastScannedDate { get; set; }
        public string WatchedParent { get; set; }
    }

    /// <summary>
    /// This DB table is used to track all photos contained in Micasa Watched or One Tine 
    /// folder.
    /// </summary>
    public class PhotosTbl
    {
        public int Id { get; set; }
        public string Picture { get; set; }
        public string Caption { get; set; }
        public string FileType { get; set; }
        public string FQFilename { get; set; }
        public string Pathname { get; set; }
        public DateTime ModificationDate { get; set; }
        public string[] Faces { get; set; }
        public string[] Albums { get; set; }

        /// <summary>
        /// Compare two DateTime objects.  This function is needed to work around
        /// the fact that LiteDB stores dates as UTC milliseconds since the Unix 
        /// epoch; which is less precision than the C# DateTime object contains.  
        /// So, this function limits its comparison to milliseconds; that is, it 
        /// doesn't compare the ticks.
        /// </summary>
        /// <param name="date1">A DateTime</param>
        /// <param name="date2">A DateTime</param>
        /// <returns>true if </returns>
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
    }
}
