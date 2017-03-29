//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

// TODO: Github release
// TODO: Publish (incl. social media links)
// TODO: Post on handmade network forums, social media, work email

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
