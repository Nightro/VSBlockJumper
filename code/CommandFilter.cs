//------------------------------------------------------------------------------
// <copyright file="CommandFilter.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;

namespace VSBlockJumper
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal class TextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import(typeof(IVsEditorAdaptersFactoryService))]
        private IVsEditorAdaptersFactoryService m_editorAdaptersFactory = null;

        [Import(typeof(ISmartIndentationService))]
        private ISmartIndentationService m_smartIndentation = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            CommandFilter filter = new CommandFilter(textView, m_smartIndentation);
            IVsTextView view = m_editorAdaptersFactory.GetViewAdapter(textView);
            int result = view.AddCommandFilter(filter, out IOleCommandTarget next);
            if (result == VSConstants.S_OK)
            {
                filter.Next = next;
            }
        }
    }

    public class CommandFilter : IOleCommandTarget
    {
        private enum JumpDirection
        {
            Up = -1,
            Down = 1
        }

        private ISmartIndentationService SmartIndentation { get; }
        private IWpfTextView View { get; set; }
        public IOleCommandTarget Next { get; set; }

        public CommandFilter(IWpfTextView view, ISmartIndentationService smartIndentation)
        {
            View = view;
            SmartIndentation = smartIndentation;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == IDs.PackageCommandSetGUID)
            {
                for (int i = 0; i < prgCmds.Length; ++i)
                {
                    if (prgCmds[i].cmdID == IDs.JumpUpCommandID ||
                        prgCmds[i].cmdID == IDs.JumpDownCommandID ||
                        prgCmds[i].cmdID == IDs.JumpSelectUpCommandID ||
                        prgCmds[i].cmdID == IDs.JumpSelectDownCommandID)
                    {
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                    }
                }

                return VSConstants.S_OK;
            }

            return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == IDs.PackageCommandSetGUID)
            {
                if (nCmdID == IDs.JumpUpCommandID)
                {
                    Jump(JumpDirection.Up);
                }
                else if (nCmdID == IDs.JumpDownCommandID)
                {
                    Jump(JumpDirection.Down);
                }
                else if (nCmdID == IDs.JumpSelectUpCommandID)
                {
                    JumpSelect(JumpDirection.Up);
                }
                else if (nCmdID == IDs.JumpSelectDownCommandID)
                {
                    JumpSelect(JumpDirection.Down);
                }

                return VSConstants.S_OK;
            }

            return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void JumpSelect(JumpDirection direction)
        {
            // choose our selection start point for the selection right before we jump
            VirtualSnapshotPoint start = View.Caret.Position.VirtualBufferPosition;
            if (!View.Selection.IsEmpty)
            {
                // we already have a selection, if our caret is at a higher position
                // than the end of our selection, then our selection starts at our end
                // point - all other possible cases (i.e. our caret is > selection start)
                // the selection start is our start point
                start = View.Selection.Start;
                if (View.Caret.Position.VirtualBufferPosition < View.Selection.End)
                {
                    start = View.Selection.End;
                }
            }
            
            Jump(direction);

            // use the new caret position as our selection end point
            VirtualSnapshotPoint end = View.Caret.Position.VirtualBufferPosition;
            View.Selection.Select(start, end);
        }

        private void Jump(JumpDirection direction)
        {
            // as with the standard caret moving operations (click, arrow keys, etc.) 
            // we clear our current selection when we jump
            View.Selection.Clear();

            // if the line we begin on is text, navigate to the next blank line
            // if the line we begin on is blank, and the next line is also blank
            // then navigate to the next blank line right before a text line
            // If we find no suitable lines, we navigate to the last line
            CaretPosition startingPos = View.Caret.Position;
            ITextBuffer buffer = View.TextBuffer;
            ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
            SnapshotPoint start = startingPos.BufferPosition;

            ITextSnapshotLine previousLine = start.GetContainingLine();
            ITextSnapshotLine targetLine = null;
            bool previousLineIsBlank = string.IsNullOrWhiteSpace(previousLine.GetTextIncludingLineBreak());
            int startLineNo = currentSnapshot.GetLineNumberFromPosition(start.Position);
            int lineInc = (int)direction;
            int firstLine = startLineNo + lineInc;
            for (int i = firstLine; i >= 0 && i < currentSnapshot.LineCount; i += lineInc)
            {
                ITextSnapshotLine line = currentSnapshot.GetLineFromLineNumber(i);
                string lineContents = line.GetTextIncludingLineBreak();
                bool lineIsBlank = string.IsNullOrWhiteSpace(lineContents);
                if (lineIsBlank)
                {
                    if (!previousLineIsBlank)
                    {
                        // found our next blank line outside our block
                        targetLine = line;
                        break;
                    }
                }
                else if (previousLineIsBlank && i != firstLine)
                {
                    // found our block, go to the blank line right before it
                    targetLine = previousLine;
                    break;
                }

                previousLine = line;
                previousLineIsBlank = lineIsBlank;
            }

            // we found no suitable line so just choose the last line in the direciton 
            // we were moving (first or last line of the file)
            if (targetLine == null)
            {
                targetLine = previousLine;
            }

            // move the caret to the blank line indented with the appropriate number of virtual spaces
            int? virtualSpaces = SmartIndentation.GetDesiredIndentation(View, targetLine);
            VirtualSnapshotPoint finalPosition = new VirtualSnapshotPoint(targetLine.Start, virtualSpaces.GetValueOrDefault());
            View.Caret.MoveTo(finalPosition);

            // scroll our view to the new caret position
            View.Caret.EnsureVisible();
        }
    }
}
