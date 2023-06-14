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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Micasa
{
    using IDisposable = System.IDisposable;
    using FrameworkElement = System.Windows.FrameworkElement;
    using Cursor = System.Windows.Input.Cursor;
    using Cursors = System.Windows.Input.Cursors;
    using Debug = System.Diagnostics.Debug;

    /// <summary>
    /// Display the busy cursor while a task is running, and then restore the original 
    /// mouse cursor when the task completes.
    /// 
    /// Author: Sergey Alexandrovich Kryukov (https://www.sakryukov.org/).
    /// URL: https://www.codeproject.com/Tips/137802/Hourglass-Mouse-Cursor-Always-Changes-Back-to-its
    /// License: The Code Project Open License (CPOL) 1.02 (https://www.codeproject.com/info/cpol10.aspx).
    /// </summary>
    sealed public class WaitCursorIndicator : IDisposable
    {
        /// <summary>
        /// Display the busy cursor while a task is running, and then restore the original 
        /// mouse cursor when the task completes.
        /// 
        /// Usage:
        ///     using(new WaitCursorIndicator(owner)) {
        ///          ... some long-running code here
        ///     }
        /// </summary>
        /// <param name="owner">The FrameworkElement that owns the process.</param>
        public WaitCursorIndicator(FrameworkElement owner)
        {
            this.Onwer = owner;
            Debug.Assert(
                owner != null,
                "WaitCursorIndicator expects non-null argument");
            if (owner == null)
            {
                return;
            }
            Previous = owner.Cursor;
            owner.Cursor = Cursors.Wait;
        } //WaitCursorIndicator

        void IDisposable.Dispose()
        {
            if (this.Onwer == null)
            {
                return;
            }
            this.Onwer.Cursor = Previous;
        } //IDisposable.Dispose

#pragma warning disable IDE0044 // Add readonly modifier
        FrameworkElement Onwer;
        Cursor Previous;
#pragma warning restore IDE0044 // Add readonly modifier
    } //class WaitCursorIndicator
}
