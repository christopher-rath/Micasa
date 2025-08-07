#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2025 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–?Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System.Reflection;
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
    ),
    AssemblyVersion("0.12.*") // --> Also update in "MainWindow.xaml.cs" <--
    ]
