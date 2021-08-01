using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Micasa
{
    class Folder
    {
        /// <summary>
        /// Each Folder instance represents a unique pathname (that is, drive letter plus full pathname)
        /// along with any dot-files, subfolders, and photos contained in the folder (directory).  The 
        /// intent is to create Folder instances in real time rather than (a) creating them in advance of 
        /// a filesystem folder being accessed or (b) caching them in the database; however, a Folder
        /// instance will update the database as it writes changes to dot-files, pictures, or other real 
        /// objects represented by the instance's properties.
        /// </summary>
        private readonly string thePathname;
        private string[] subfolderPaths;

        public Folder(string Pathname, bool CreateIfDoesNotExist = false)
        {
            thePathname = Pathname;

            if (Directory.Exists(thePathname))
            {
                subfolderPaths = Directory.GetDirectories(thePathname);
            }
            else
            {
                // Since the Pathname doesn't exist, either create the folder
                // or throw and exception, depending upon the value of 
                // CreateIfDoesNotExist.
                if (CreateIfDoesNotExist)
                {
                    Directory.CreateDirectory(thePathname);
                }
                else
                {
                    throw new DirectoryNotFoundException("The folder " + thePathname + " does not exist.");
                }
            }
        }
    }
}
