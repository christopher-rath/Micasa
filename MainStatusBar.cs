using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Micasa
{
    /// <summary>
    /// The MainWindow's status bar class.  All updates to the actual status bar
    /// XAML widget are centralised in this class.  Any code that needs to update
    /// the status bar widget does so through this class.
    /// </summary>
    internal class MainStatusBar
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
