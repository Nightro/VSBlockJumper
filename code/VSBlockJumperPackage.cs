//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

// TODO: Commit current changes and merge main back into development
// TODO: Use development to switch to VS 14
// TODO: Do a gif demoing the functionality
// TODO: Do a concise description and mention inspirations
// TODO: Publish
// TODO: Github release
// TODO: Post on handmade network forums


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
