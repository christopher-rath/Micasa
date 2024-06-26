﻿#region Copyright
// WaitCursorIndicagtor -- Display the busy cursor while a task is running, and then restore the original 
//    mouse cursor when the task completes.
// Author: Sergey Alexandrovich Kryukov (https://www.sakryukov.org/)
// Archived at: https://www.codeproject.com/Tips/137802/Hourglass-Mouse-Cursor-Always-Changes-Back-to-its
// Copyright 2010 © Sergey Alexandrovich Kryukov
// License: The Code Project Open License (CPOL) 1.02 (https://www.codeproject.com/info/cpol10.aspx)
// Warranty: None, see the license.
#endregion

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
