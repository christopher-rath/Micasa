#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2023 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System.IO;

namespace Micasa
{
    sealed class Folder
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
        private readonly string[] subfolderPaths;

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
