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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace RichTextBoxExtensions
{
    /// <summary>
    /// Extensions to the WPF RichTextBox control; with a view to extending its functionality along the
    /// lines of the Windows Forms rich text control.
    /// </summary>
    
    public static class RichTextBoxExtensions
    {
        /// <summary>
        /// Scan the content of a RichTextBox control and make any https URLs
        /// clickable.  The initial version of this method was written by Bing Chat,
        /// and then tidied up by me and Intellicode.
        /// </summary>
        /// <param name="self"></param>
        public static void MakeUrlsClickable(this RichTextBox self)
        {
            TextPointer pointer = self.Document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, @"(https?://[^\s]+)");
                    foreach (Match match in matches.Cast<Match>())
                    {
                        TextPointer start = pointer.GetPositionAtOffset(match.Index);
                        TextPointer end = start.GetPositionAtOffset(match.Length);
                        Hyperlink hyperlink = new(start, end)
                        {
                            NavigateUri = new Uri(match.Value)
                        };
                    }
                }
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        /// <summary>
        /// This extension provides the ability to use a WPF RichTextBox control in a WinForm-esque style:
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