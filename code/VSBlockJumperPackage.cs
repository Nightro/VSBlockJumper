//------------------------------------------------------------------------------
// <copyright file="VSBlockJumperPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;

// TODO: Refactor to be like https://github.com/noahric/alignassignments with keyboard input (copy his way of getting the view too)
// TODO: Test that selecting the text works... else implement that - might have to do view.Selection.Select(start, end) - might have to include existing selection
// TODO: Clean up deluge of comments, add file headers/real comments
// TODO: Remove unused references/using statements
// TODO: Publish with a basic writeup
// TODO: Nuget packages my boi - sort er out eh

namespace VSBlockJumper
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(VSBlockJumperPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSBlockJumperPackage : Package
    {
        /// <summary>
        /// VSBlockJumperPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "c1f1672f-9ea9-46c0-a1a3-78c6daae19ab";

        /// <summary>
        /// </summary>
        public VSBlockJumperPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            JumpDown.Initialize(this);
            JumpUp.Initialize(this);
            base.Initialize();
        }

        /// <summary>
        /// Returns the currently active view if it is a IWpfTextView.
        /// </summary>
        public IWpfTextView GetCurrentView()
        {
            // The SVsTextManager is a service through which we can get the active view.
            var textManager = (IVsTextManager)Package.GetGlobalService(typeof(SVsTextManager));
            IVsTextView textView;
            textManager.GetActiveView(1, null, out textView);

            // Now we have the active view as IVsTextView, but the text interfaces we need
            // are in the IWpfTextView.
            var userData = (IVsUserData)textView;
            if (userData == null)
                return null;
            Guid guidWpfViewHost = DefGuidList.guidIWpfTextViewHost;
            object host;
            userData.GetData(ref guidWpfViewHost, out host);
            return ((IWpfTextViewHost)host).TextView;
        }

        public void Jump(int direction)
        {
            IWpfTextView view = GetCurrentView();
            if (view == null)
            {
                return;
            }

            CaretPosition currentPos = view.Caret.Position;
            ITextBuffer buffer = view.TextBuffer;
            ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
            SnapshotPoint start = currentPos.BufferPosition;

            // if first line is a blank space
            // we need to pick the line right before a text line
            // unless its our line, in which case we can then select the next space line

            ITextSnapshotLine previousLine = start.GetContainingLine();
            bool previousLineIsBlank = string.IsNullOrWhiteSpace(previousLine.GetTextIncludingLineBreak());
            int startLineNo = currentSnapshot.GetLineNumberFromPosition(start.Position);
            int lineInc = Math.Sign(direction);
            int firstLine = startLineNo + lineInc;
            for (int i = firstLine; i < currentSnapshot.LineCount; i += lineInc)
            {
                ITextSnapshotLine line = currentSnapshot.GetLineFromLineNumber(i);
                string lineContents = line.GetTextIncludingLineBreak();
                bool lineIsBlank = string.IsNullOrWhiteSpace(lineContents);
                if (lineIsBlank)
                {
                    if (!previousLineIsBlank)
                    {
                        view.Caret.MoveTo(line.End);
                        break;
                    }
                }
                else if (previousLineIsBlank && i != firstLine)
                {
                    // we skip blocks of whitespace lines to the last whitespace line above a text block
                    view.Caret.MoveTo(previousLine.End);
                    break;
                }

                previousLine = line;
                previousLineIsBlank = lineIsBlank;
            }
        }

        #endregion
    }
}
