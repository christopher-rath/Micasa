﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Personal coding style preference is to explicitly assign a value, even if that assignment is redundant.", Scope = "member", Target = "~M:Micasa.Database.GetCaptionFromImage(System.String)~System.String")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.Database.CreateDB")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.Database.GetCaptionFromImage(System.String)~System.String")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.PictureScanner.StartScanner(System.Object)")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.PictureWatcher.StartProcessor(System.Object)")]
[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Personal coding style preference is to explicitly assign a value, even if that assignment is redundant.", Scope = "member", Target = "~M:Micasa.PictureWatcher.StartProcessor(System.Object)")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.DeletedScanner.StartScanner(System.Object)")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:RichtextboxExtensions.RTBExtensions.SetRtf(System.Windows.Controls.RichTextBox,System.String)")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.MainWindow.IsDirectoryWritable(System.String,System.Boolean)~System.Boolean")]
[assembly: SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Intellicode says this surpession is unnecessary; but, if it's removed then Intellicode will suggest other changes that break the singleton pattern I'm using here.", Scope = "member", Target = "~M:Micasa.MainStatusBar.#ctor")]
[assembly: SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Intellicode says this surpession is unnecessary; but, if it's removed then Intellicode will suggest other changes that break the singleton pattern I'm using here.", Scope = "type", Target = "~T:Micasa.MainStatusBar")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Personal coding style preference is to use the robust using{} statement.", Scope = "member", Target = "~M:Micasa.OptionsWindow.Rebuild_Click(System.Object,System.Windows.RoutedEventArgs)")]
