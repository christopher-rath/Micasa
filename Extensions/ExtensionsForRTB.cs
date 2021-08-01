#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RichtextboxExtensions
{
    /// <summary>
    /// Extensions to the WPF Richtextbox control; with a view to extending its functionality along the
    /// lines of the Windows Forms rich text control.
    /// </summary>
    
    public static class RTBExtensions
    {
        /// <summary>
        /// This extension provides the ability to use a WPF Richtextbox control in a WinForm-esque style:
        ///     <code>richTextBox1.SetRtf(rtf);</code>
        /// </summary>
        /// <param name="rtbCtrl">the Richtextcontrol</param>
        /// <param name="theRTStr">the RTF string to assign</param>
        public static void SetRtf(this RichTextBox rtbCtrl, string theRTStr)
        {
            var strBytes = Encoding.UTF8.GetBytes(theRTStr);
            using (var reader = new MemoryStream(strBytes))
            {
                reader.Position = 0;
                rtbCtrl.SelectAll();
                rtbCtrl.Selection.Load(reader, DataFormats.Rtf);
            }
        }
    }
}