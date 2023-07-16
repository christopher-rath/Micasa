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

namespace Micasa
{
    /// <summary>
    /// The MainWindow's status bar class.  All updates to the actual status bar
    /// XAML widget are centralised in this class.  Any code that needs to update
    /// the status bar widget does so through this class.
    /// </summary>
    internal sealed class MainStatusBar
    {
        private string _StatusBarMsg { get; set; }
#pragma warning disable CA2211
        // The single instance of MainStatusBar.
        public static MainStatusBar Instance = new();
#pragma warning restore CA2211

        private MainStatusBar()
        {
            // Nothing yet.
        }

        /// <summary>
        /// Update the MainWindow's statusbar message.  The setter both saves
        /// the text in _StatusBarMsg and posts the text to the widget.
        /// </summary>
        public string StatusBarMsg
        { 
            get => _StatusBarMsg;

            set
            {
                _StatusBarMsg = value;
                MainWindow.Instance.Dispatcher.Invoke((Action)(() =>
                {
                    MainWindow.Instance.tbStatusMsg.Text = _StatusBarMsg;
                }));
            }
        } 
    }
}
