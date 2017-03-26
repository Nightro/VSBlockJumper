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
    internal class TestViewCreationListener : IWpfTextViewCreationListener
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
            VirtualSnapshotPoint start = View.Caret.Position.VirtualBufferPosition;
            if (!View.Selection.IsEmpty)
            {
                start = View.Selection.Start;
                if (View.Caret.Position.VirtualBufferPosition <= View.Selection.Start)
                {
                    start = View.Selection.End;
                }
            }
            
            Jump(direction);
            VirtualSnapshotPoint end = View.Caret.Position.VirtualBufferPosition;
            View.Selection.Select(start, end);
        }

        private void Jump(JumpDirection direction)
        {
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
                        View.Caret.MoveTo(line.Start);
                        break;
                    }
                }
                else if (previousLineIsBlank && i != firstLine)
                {
                    View.Caret.MoveTo(previousLine.Start);
                    break;
                }

                previousLine = line;
                previousLineIsBlank = lineIsBlank;
            }

            if (View.Caret.Position == startingPos)
            {
                View.Caret.MoveTo(previousLine.Start);
            }

            // scroll our view to the new caret position
            View.Caret.EnsureVisible();
        }
    }
}
