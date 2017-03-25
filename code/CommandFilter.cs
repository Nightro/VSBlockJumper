using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace VSBlockJumper
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class VsTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import(typeof(IVsEditorAdaptersFactoryService))]
        private IVsEditorAdaptersFactoryService m_editorFactory = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            IVsTextView view = m_editorFactory.GetViewAdapter(textView);

            CommandFilter filter = new CommandFilter(textView);

            IOleCommandTarget next;
            int result = view.AddCommandFilter(filter, out next);

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

        private IWpfTextView View { get; set; }
        public IOleCommandTarget Next { get; set; }

        public CommandFilter(IWpfTextView view)
        {
            View = view;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == IDs.PackageCommandSetGUID)
            {
                for (int i = 0; i < prgCmds.Length; ++i)
                {
                    if (prgCmds[i].cmdID == IDs.JumpUpCommandID ||
                        prgCmds[i].cmdID == IDs.JumpDownCommandID)
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

                return VSConstants.S_OK;
            }

            return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void Jump(JumpDirection direction)
        {
            CaretPosition currentPos = View.Caret.Position;
            ITextBuffer buffer = View.TextBuffer;
            ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
            SnapshotPoint start = currentPos.BufferPosition;

            // if first line is a blank space
            // we need to pick the line right before a text line
            // unless its our line, in which case we can then select the next space line

            ITextSnapshotLine previousLine = start.GetContainingLine();
            bool previousLineIsBlank = string.IsNullOrWhiteSpace(previousLine.GetTextIncludingLineBreak());
            int startLineNo = currentSnapshot.GetLineNumberFromPosition(start.Position);
            int lineInc = (int)direction;
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
                        View.Caret.MoveTo(line.Start);
                        break;
                    }
                }
                else if (previousLineIsBlank && i != firstLine)
                {
                    // we skip blocks of whitespace lines to the last whitespace line above a text block
                    View.Caret.MoveTo(previousLine.Start);
                    break;
                }

                previousLine = line;
                previousLineIsBlank = lineIsBlank;
            }
        }
    }
}
