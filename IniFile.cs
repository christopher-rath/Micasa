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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Micasa
{
    /// <summary>
    /// A class for reading and writing values by section and key contained in a standard ".ini" initialization file.
    /// Using the class in your program is as simple as instantiating it and calling the various reader/writer methods.
    /// For example:
    ///     <code>
    ///         var iniFile = new IniFile("MyFile.ini");
    ///         string font = iniFile.GetValue("Text Style", "Font", "Arial");
    ///         int size = iniFile.GetInteger("Text Style", "Size", 12);
    ///         bool bold = iniFile.GetBoolean("Text Style", "Bold", false); 
    ///     </code>
    /// </summary>
    /// <remarks>
    /// .ini file section and key names are not case-sensitive
    /// Sections in the initialization file must have the following form:
    ///     <code>
    ///         ; Optional comment line
    ///         [Section]
    ///         Key=Value
    ///     </code>
    /// This class is modeled after one posted to by Bruce Greene:
    /// https://www.codeproject.com/Tips/771772/A-Simple-and-Efficient-INI-File-Reader-in-Csharp
    /// However, his code implemented a simple parser only supported reading .ini files.  This new class
    /// both reads and writes .ini files, and leverages native Win32 methods.  The way in which
    /// the Win32 methods can be access was taken from code posted by an unnamed coder:
    /// https://www.codeproject.com/Articles/1990/INI-Class-using-C
    /// </remarks>
    public class IniFile
    {        
        private const string defaultCommentDelimiter = ";";
        private const string strFalse = "False";
        private const string strTrue = "True";

        /// <summary>
        ///     WritePrivateProfileString sets a value inside of an INI file. This function can also be used to set 
        ///     numerical values if they are in string form, for example using "1" to represent the number 1. If the 
        ///     INI file you try to write to does not exist, it will be created. Likewise, if the section or value 
        ///     does not exist, it will also be created. The function returns 0 if an error occurs, or 1 if successful. 
        ///     Note that INI file support is only provided in Windows for backwards compatibility; using the registry 
        ///     to store information is preferred.
        /// </summary>
        /// <param name="lpAppName">The name of the section to which the string will be copied. If the section does 
        ///     not exist, it is created. The name of the section is case-independent; the string can be any 
        ///     combination of uppercase and lowercase letters.</param>
        /// <param name="lpKeyName">The name of the key to be associated with a string. If the key does not exist 
        ///     in the specified section, it is created. If this parameter is NULL, the entire section, including 
        ///     all entries within the section, is deleted.</param>
        /// <param name="lpString">A null-terminated string to be written to the file. If this parameter is NULL, 
        /// the key pointed to by the lpKeyName parameter is deleted.</param>
        /// <param name="lpFileName">The name of the initialization file.  If the file was created using Unicode 
        ///     characters, the function writes Unicode characters to the file.  Otherwise, the function writes ANSI 
        ///     characters.</param>
        /// <returns>
        ///     If the function successfully copies the string to the initialization file, the return value is nonzero.
        ///     If the function fails, or if it flushes the cached version of the most recently accessed initialization 
        ///     file, the return value is zero.  To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        ///     If the lpFileName parameter does not contain a full path and file name for the file, 
        ///     WritePrivateProfileString searches the Windows directory for the file. If the file does not exist, this 
        ///     function creates the file in the Windows directory.
        ///     
        ///     MS documentation for this function: 
        ///     https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringa
        ///     <code>
        ///         BOOL WritePrivateProfileStringA(
        ///             LPCSTR lpAppName,
        ///             LPCSTR lpKeyName,
        ///             LPCSTR lpString,
        ///             LPCSTR lpFileName
        ///         );
        ///     </code>
        ///     Note that BOOL is type int, and LPCSTR is string.
        /// </remarks>
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("kernel32")]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        /// <summary>
        ///     GetPrivateProfileString reads an string value from an INI file. The parameters passed to the function 
        ///     specify which value will be read from. The function always returns the length in characters of the string 
        ///     put into the variable passed as lpReturnedString. If the function was successful, the string read from the 
        ///     INI file will be put into lpReturnedString. If not, it will instead receive the string given as lpDefault. 
        ///     Note that INI file support is only provided in Windows for backwards compatibility; using the registry to 
        ///     store information is preferred.
        /// </summary>
        /// <param name="lpAppName">The name of the section containing the key name. If this parameter is NULL, the 
        ///     GetPrivateProfileString function copies all section names in the file to the supplied buffer.</param>
        /// <param name="lpKeyName">The name of the key whose associated string is to be retrieved. If this parameter 
        ///     is NULL, all key names in the section specified by the lpAppName parameter are copied to the buffer 
        ///     specified by the lpReturnedString parameter.</param>
        /// <param name="lpDefault">A default string. If the lpKeyName key cannot be found in the initialization file, 
        ///     GetPrivateProfileString copies the default string to the lpReturnedString buffer.  If this parameter is 
        ///     NULL, the default is an empty string, "".  Avoid specifying a default string with trailing blank 
        ///     characters.The function inserts a null character in the lpReturnedString buffer to strip any trailing 
        ///     blanks.</param>
        /// <param name="lpReturnedString">A pointer to the fixed length buffer that receives the retrieved string.</param>
        /// <param name="nSize">The size of the buffer pointed to by the lpReturnedString parameter, in characters.</param>
        /// <param name="lpFileName">The name of the initialization file. If this parameter does not contain a full 
        ///     path to the file, the system searches for the file in the Windows directory.</param>
        /// <returns>
        ///     The return value is the number of characters copied to the buffer, not including the terminating null 
        ///     character.  If neither lpAppName nor lpKeyName is NULL and the supplied destination buffer is too small 
        ///     to hold the requested string, the string is truncated and followed by a null character, and the return 
        ///     value is equal to nSize minus one.  If either lpAppName or lpKeyName is NULL and the supplied destination 
        ///     buffer is too small to hold all the strings, the last string is truncated and followed by two null 
        ///     characters.  In this case, the return value is equal to nSize minus two.  In the event the initialization 
        ///     file specified by lpFileName is not found, or contains invalid values, calling GetLastError will return 
        ///     '0x2' (File Not Found). To retrieve extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        ///     MS documentation for this function:
        ///     https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring
        ///     <code>
        ///         DWORD GetPrivateProfileString(
        ///             LPCTSTR lpAppName,
        ///             LPCTSTR lpKeyName,
        ///             LPCTSTR lpDefault,
        ///             LPTSTR lpReturnedString,
        ///             DWORD nSize,
        ///             LPCTSTR lpFileName
        ///         );
        ///     </code>
        ///     Note that DWARD is uint, and LPCTCSTR is string
        /// </remarks>
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("kernel32")]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable CA1838 // Avoid 'StringBuilder' parameters for P/Invokes
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
#pragma warning restore CA1838 // Avoid 'StringBuilder' parameters for P/Invokes
        /// <summary>
        ///     GetPrivateProfileInt reads an integer value from any INI file. The parameters passed to the function 
        ///     specify which value will be read from. If successful, the function returns the value read. If the value 
        ///     you specify does not exist or is a string (i.e., not a number), the value specified as nDefault is returned. 
        ///     Note that INI file support is only provided in Windows for backwards compatibility; using the registry to 
        ///     store information is preferred.
        /// </summary>
        /// <param name="lpAppName">The name of the section in the initialization file.</param>
        /// <param name="lpKeyName">The name of the key whose value is to be retrieved. This value is in the form of a 
        ///     string; the GetPrivateProfileInt function converts the string into an integer and returns the integer.</param>
        /// <param name="nDefault">The default value to return if the key name cannot be found in the initialization file.</param>
        /// <param name="lpFileName">The name of the initialization file. If this parameter does not contain a full path 
        ///     to the file, the system searches for the file in the Windows directory.</param>
        /// <returns>
        ///     The return value is the integer equivalent of the string following the specified key name in the specified 
        ///     initialization file. If the key is not found, the return value is the specified default value.
        /// </returns>
        /// <remarks>
        ///     MS documentation for this function:
        ///     https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofileint
        ///     <code>
        ///         UINT GetPrivateProfileInt(
        ///             LPCTSTR lpAppName,
        ///             LPCTSTR lpKeyName,
        ///             INT nDefault,
        ///             LPCTSTR lpFileName
        ///         );
        ///     </code>
        ///     Note that UINT is uint, INT is int, and LPCTCSTR is string
        /// </remarks>
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("kernel32")]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        /// <summary>
        ///     Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        /// <param name="filename">The initialization file path.</param>
        /// <param name="commentDelimiter">The comment delimiter string (default value is 
        ///     <see cref="defaultCommentDelimiter"/>).</param>
        public IniFile(string filename, string commentDelimiter = defaultCommentDelimiter)
        {
            CommentDelimiter = commentDelimiter;
            TheFile = filename;
        }

        /// <summary>
        /// The comment delimiter string (default value is <see cref="defaultCommentDelimiter"/>).
        /// </summary>
        public string CommentDelimiter { get; set; }

        private string theFile = null;
        /// <summary>The initialization file path.</summary>
        public string TheFile
        {
            get { return theFile; }
            set
            {
                theFile = null;
                if (File.Exists(value))
                {
                    theFile = value;
                    // To Do: Need to check that we can read and write the file and throw
                    // an error if we can't.
                }
                else
                {
                    theFile = value;
                    // To Do: Need to check that we can write a file to the folder and throw
                    // an error if we can't.
                }
            }
        }

        /// <summary>Gets a boolean value by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value from the INI file or the defaultValue.</returns>
        /// <remarks>
        ///     Always check for opposite of the default value in the .ini file, and then set
        ///     the variable accordingly.  This ensures that the default value reigns unless the user
        ///     has specifically chosen the correct non-default value.
        /// </remarks>
        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            string temp;
            bool rtnVal;
            string defStr = strFalse;

            if (defaultValue)
            {
                defStr = strTrue;
            }

            temp = GetString(section, key, defStr);

            if (string.Equals(strTrue, temp, StringComparison.OrdinalIgnoreCase))
            {
                rtnVal = true;
            }
            else if (string.Equals(strFalse, temp, StringComparison.OrdinalIgnoreCase))
            {
                rtnVal = false;
            }
            else
            {
                rtnVal = defaultValue;
            }
            
            return rtnVal;
        }

        /// <summary>Gets a DateeTime value by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        ///     The value from the INI file or the defaultValue.  If the string to DateTime
        ///     conversion fails (e.g., overflow) then the defaultValue is returned.
        /// </returns>
        public DateTime GetDateTime(string section, string key, DateTime defaultValue)
        {
            string temp;
            DateTime tempDT;
            // Retrieve a CultureInfo object.
            CultureInfo invC = CultureInfo.InvariantCulture;

            try
            {
                temp = GetString(section, key, defaultValue.ToString("O"));
                tempDT = DateTime.Parse(temp, invC);
            }
            catch
            {
                tempDT = defaultValue;
            }
            
            return tempDT;
        }

        /// <summary>Gets an integer value by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        ///     The value from the INI file or the defaultValue.  If the string to integer
        ///     conversion fails (e.g., overflow) then the defaultValue is returned.
        /// </returns>
        public int GetInteger(string section, string key, int defaultValue = 0)
        {
            int temp;

            try
            {
                temp = Convert.ToInt32(GetPrivateProfileInt(section, key, defaultValue, TheFile));
            }
            catch
            {
                temp = defaultValue;
            }
            return temp;
        }

        /// <summary>
        ///     Gets a string value by section and key.  The string length is limited to 255 characters.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value from the INI file or the defaultValue.</returns>
        public string GetString(string section, string key, string defaultValue = "")
        {
            StringBuilder temp = new(255);
            _ = GetPrivateProfileString(section, key, defaultValue, temp, 255, TheFile);
            return temp.ToString();
        }

        /// <summary>Write a bool to the INI file as a string, by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The bool value to write.</param>
        /// <returns>true if successful and false if there was an error.</returns>
        public bool SetBool(string section, string key, bool value)
        {
            string strValue = strFalse;

            if (value)
            {
                strValue = strTrue;
            }
            return SetString(section, key, strValue);
        }

        /// <summary>Write a DateTime to the INI file as a string, by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The DateTime value to write.</param>
        /// <returns>true if successful and false if there was an error.</returns>
        public bool SetDateTime(string section, string key, DateTime value)
        {
            string strValue;

            try
            {
                strValue = value.ToString("O");
            }
            catch
            {
                return false;
            }

            return SetString(section, key, strValue);
        }

        /// <summary>Write an integer to the INI file as a string, by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The integer value to write.</param>
        /// <returns>true if successful and false if there was an error.</returns>
        public bool SetInteger(string section, string key, int value)
        {
            string strValue;
            // Retrieve a CultureInfo object.
            CultureInfo invC = CultureInfo.InvariantCulture;

            try
            {
                strValue = value.ToString(invC);
            }
            catch
            {
                return false;
            }

            return SetString(section, key, strValue);
        }

        /// <summary>Write a string to the INI file by section and key.</summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The string to write.</param>
        /// <returns>true if successful and false if there was an error.</returns>
        public bool SetString(string section, string key, string value)
        {
            bool rtnVal = false;

            if (0 != WritePrivateProfileString(section, key, value, TheFile))
            {
                rtnVal = true;
            }
            return rtnVal;
        }
    }
}
