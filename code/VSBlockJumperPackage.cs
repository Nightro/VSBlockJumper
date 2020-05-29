//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Task = System.Threading.Tasks.Task;

namespace VSBlockJumper
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(IDs.PackageGUIDString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), "VSBlockJumper", "General", 113, 116, true)]
    [ProvideProfileAttribute(typeof(OptionPageGrid), "VSBlockJumper", "VSBlockJumper", 113, 113, isToolsOptionPage: true, DescriptionResourceID = 115)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSBlockJumperPackage : AsyncPackage
    {
        protected async override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // this sucks - the options page grid needs access to the OptionsService when the options are loaded
            // however, I can't use GetService in the constructor for the OptionPageGrid because the Site is not
            // yet initialised - by the time it IS initialised, we also load our options - I think this is the
            // safest way (if not cleanest) to ensure we have an options service ready to push our data into
            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            OptionPageGrid.OptionsService = componentModel.GetService<IEditorOptionsFactoryService>();

            // this also sucks - here we force the options to be loaded as they are otherwise NOT loaded even when 
            // the package and MEF component ARE loaded - the only time it auto loads is if we go into the options page
            OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
        }
    }
}
