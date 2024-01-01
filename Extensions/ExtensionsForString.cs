#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2024 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System.Collections.Generic;

namespace StringExtensions
{
    /// <summary>
    /// Extensions to the string class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Examines a folder name to see if it is a special directory that should
        /// not be exposed to the user interface (e.g., a recycle bin).  Because a 
        /// drive letter may precede the folder name, we only test to see if the name 
        /// ends in the reserved name.  An improvement to this method would be to 
        /// first remove any preceding drive letter and the perform the test.
        /// </summary>
        /// <param name="s">The folder name to test.</param>
        /// <returns>True if the folder is a special directory; otherwise, False.</returns>
        public static bool IsSpecialDir(this string s)
        {
            bool rtnVal = false;
            var specialDirs = new List<string> { "$Recycle.Bin", "$SysReset", "$WinREAgent" };

            foreach (var sd in specialDirs)
            {
                try
                {
                    if (s.EndsWith(sd, System.StringComparison.Ordinal))
                    {
                        rtnVal = true;
                        break;
                    }
                }
                catch
                {
                    // We mask any error from the caller and simply stop checking.
                    break;
                }
            }

            return rtnVal;
        }

        /// <summary>
        /// Remove a prefix from the front of a string.  If the prefix does not 
        /// occur, then simmply return the original string.
        /// </summary>
        /// <param name="s">String to remove the prefix from.</param>
        /// <param name="p">The prefix string to remove.</param>
        /// <returns>The modified string.</returns>
        public static string RmPrefix(this string s, string p)
        {
            string rtnVal = s;

            if (s.StartsWith(p, System.StringComparison.Ordinal))
            {
                rtnVal = s[p.Length..];
            }
            return rtnVal;
        }
    }
}
