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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RichTextBoxExtensions
{
    /// <summary>
    /// Extensions to the WPF RichTextBox control; with a view to extending its functionality along the
    /// lines of the Windows Forms rich text control.
    /// </summary>

    public static class RichTextBoxExtensions
    {
        /// <summary>
        /// Scan the content of a RichTextControl and convert the email address passed to it to
        /// a clickable mailto: link.  This method uses a case-invariant match to find the email
        /// address(es) to convert.
        /// </summary>
        /// <param name="self">No need to pass this; the system does it automatically.</param>
        /// <param name="email">The email address to convert to a mailto:</param>
        public static void EmailToMailto(this RichTextBox self, string email)
        {
            TextPointer pointer = self.Document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, @"(?i)" + email);
                    foreach (Match match in matches.Cast<Match>())
                    {
                        TextPointer start = pointer.GetPositionAtOffset(match.Index);
                        TextPointer end = start.GetPositionAtOffset(match.Length);
                        try
                        {
                            Hyperlink hyperlink = new(start, end)
                            {
                                NavigateUri = new Uri(@"mailto:" + match.Value)
                            };
                        }
                        catch (Exception ex)
                        {
                            // Continue execution but post a debug message.  In testing, this
                            // exception occured when we tried to insert a new Hyperlink into a 
                            // HYPERLINK field that was in the RTF we're parsing.
                            Debug.WriteLine($"In EmailToMailto(): {ex.Message}\n while handling email {match.Value}");
                        }
                    }
                }
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        /// <summary>
        /// Scan the content of a RichTextBox control and make any https URLs
        /// clickable.  The initial version of this method was written by Bing Chat,
        /// and then tidied up by me and Intellicode.
        /// 
        /// CAVEATS:
        ///  * This method only recognises http:, https:, ftp:, and mailto: URLs.
        ///  * A limitation of this method is that the regex uses whitespace to delimit 
        ///    the end of mailto URLs.
        ///  * Where HYPERLINK fields have already been encoded in the RTF, this method
        ///    may silently fail (although a debug message is output).
        /// 
        /// To use this method: 
        ///     (1) Add the following Setter to the RichTextBox control:
        ///         <RichTextBox.Resources>
        ///             <Style TargetType = "Hyperlink" >
        ///                 <Setter Property="Cursor" Value="Hand" />
        ///                 <EventSetter Event = "MouseLeftButtonDown" Handler="Hyperlink_MouseLeftButtonDown" />
        ///             </Style>
        ///         <RichTextBox.Resources>
        ///     (2) Add the following codebehind:
        ///         private void Hyperlink_MouseLeftButtonDown(object sender, MouseEventArgs e)
        ///         {
        ///             var hyperlink = (Hyperlink)sender;
        ///             Process.Start(new ProcessStartInfo(hyperlink.NavigateUri.ToString())
        ///             {
        ///                 UseShellExecute = true,
        ///             });
        ///             e.Handled = true;
        ///         }
        ///         
        /// The genisis of the codebehind and Setter code was found here:
        ///     https://stackoverflow.com/questions/762271/clicking-hyperlinks-in-a-richtextbox-without-holding-down-ctrl-wpf
        /// </summary>
        /// <param name="self">No need to pass this; the system does it automatically.</param>
        public static void MakeUrlsClickable(this RichTextBox self)
        {
            // RegexURL string from: https://urlregex.com/index.html
            const string RegexURL = @"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?";
            const string MailtoURL = @"mailto:[^\s]+";

            // Find http, https, & ftp URLs.
            MakeHyperlinks(self.Document, RegexURL);
            // Find mailto URLs.
            MakeHyperlinks(self.Document, MailtoURL);
        }

        /// <summary>
        /// Used by MakeUrlsClickable() to actually do the work of finding URls and making
        /// them clickable.  This code was moved out of MakeUrlsClickable() so that mailto:
        /// URLs could also be found and converted.
        /// </summary>
        /// <param name="doc">The RichTextBox control's content (i.e., FlowDocument).</param>
        /// <param name="regexStr">The regex to use to find URLs.</param>
        private static void MakeHyperlinks(FlowDocument doc, string regexStr)
        {
            TextPointer pointer = doc.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, regexStr);
                    foreach (Match match in matches.Cast<Match>())
                    {
                        TextPointer start = pointer.GetPositionAtOffset(match.Index);
                        TextPointer end = start.GetPositionAtOffset(match.Length);
                        try
                        {
                            Hyperlink hyperlink = new(start, end)
                            {
                                NavigateUri = new Uri(match.Value)
                            };
                        }
                        catch (Exception ex)
                        {
                            // Continue execution but post a debug message.  In testing, this
                            // exception occured when we tried to insert a new Hyperlink into a 
                            // HYPERLINK field that was in the RTF we're parsing.
                            Debug.WriteLine($"In MakeHyperlinks(): {ex.Message}\n while handling URL {match.Value}");
                        }
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