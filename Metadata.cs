#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2025 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Micasa
{
    /// <summary>
    /// All of the metadata-related methods for Miscasa are contained in this class.  Ideally,
    /// none of the rest of the code in Micasa needs to know how the metadata is stored or 
    /// retrieved.  That detail is masked by the methods in this class.
    /// </summary>
    internal class Metadata
    {
        /// <summary>
        /// Get the Caption from the image file.  This method is agnostic regarding the formatting
        /// of the caption string; that is, any RTF, HTML, or other markup is simply retrieved 
        /// from the image and returned by the method in its raw form.
        /// 
        /// The BitmapMetadata class supports GIF, JPEG, PNG, and TIFF image formats.
        /// 
        /// If any error occurs, this method will silently return an empty string.
        /// 
        /// TO DO: get any caption from the .micasa file & reconcile the two sources based on timestamps; 
        /// then return the most current caption.
        /// </summary>
        /// <param name="imgFl">Fully-qualified filename.</param>
        /// <returns>A string.</returns>
        public static string GetCaptionFromImage(string imgFl)
        {
            string caption = "";
            // Retrieve a CultureInfo object.
            CultureInfo invC = CultureInfo.InvariantCulture;
            bool supportedImg = false;

            supportedImg = Path.GetExtension(imgFl).ToLower(invC) switch
            {
                Constants.sMcFT_Gif or Constants.sMcFT_Jpg or Constants.sMcFT_JpgA or Constants.sMcFT_Png
                    or Constants.sMcFT_Tif or Constants.sMcFT_TifA => true,
                _ => false,
            };
            if (supportedImg)
            {
                try
                {
                    using (FileStream fs = File.OpenRead(imgFl))
                    {
                        BitmapSource img = BitmapFrame.Create(fs);
                        BitmapMetadata md = (BitmapMetadata)img.Metadata;

                        caption = md.Title;
                        caption ??= ""; // ??= means if 'caption' is null then do the assignment.
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
