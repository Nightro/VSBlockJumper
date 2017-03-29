//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

// TODO: Use development to switch to VS 14 (I may be able to get away with just updating the package file, worth a try anyway)
// TODO: Do a concise description: include command well commands, include navigation helper, space delimited blocks, gif, and finally mention inspirations
// TODO: Finalise manifest
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
