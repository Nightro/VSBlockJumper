//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

// TODO: Refactor to be like https://github.com/noahric/alignassignments with keyboard input (copy his way of getting the view too)
// TODO: Test that selecting the text works... else implement that - might have to do view.Selection.Select(start, end) - might have to include existing selection
// TODO: Clean up deluge of comments, add file headers/real comments
// TODO: Remove unused references/using statements
// TODO: Publish with a basic writeup
// TODO: Nuget packages my boi - sort er out eh
// TODO: Select the line at the correct indentation with virtual spaces...
// TODO: Colorizer, maybe for vscode too
// TODO: Create a 256x256 icon
// TODO: Leading whitespace
// TODO: Work required to support vs v 14

namespace VSBlockJumper
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(IDs.PackageGUIDString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSBlockJumperPackage : Package
    {
        public VSBlockJumperPackage()
        {
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
