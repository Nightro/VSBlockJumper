//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

// TODO: Work required to support vs v 14 (or even lower)
// TODO: Publish with a basic writeup
// TODO: Create a 256x256 icon

namespace VSBlockJumper
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(IDs.PackageGUIDString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSBlockJumperPackage : Package
    {
    }
}
