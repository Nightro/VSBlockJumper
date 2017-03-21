//------------------------------------------------------------------------------
// <copyright file="BlockJumper.cs" company="OzmosisGames">
//     Copyright (c) OzmosisGames.  All rights reserved.
// </copyright>
// <author>Anthony Reddan</author>
// <date>20/03/17</date>
//------------------------------------------------------------------------------

using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

namespace VSBlockJumper
{
    /// <summary>
    /// BlockJumper provides commands for jumping the carret over blocks of text
    /// to the next/previous line containing only whitespace
    /// </summary>
    internal sealed class BlockJumper : IOleCommandTarget
    {
        /// <summary></summary>
        private readonly IWpfTextView m_view;

        /// <summary></summary>
        private readonly IOleCommandTarget m_nextTarget;


        /// <summary>
        /// Initializes a new instance of the <see cref="BlockJumper"/> class.
        /// </summary>
        /// <param name="view">Text view we are operating on</param>
        public BlockJumper(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            m_view = view;
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char typedChar = char.MinValue;

            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                if (typedChar.Equals('+'))
                {
                }
            }

            return m_nextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public void JumpForward()
        {
            CaretPosition currentPos = m_view.Caret.Position;
            ITextBuffer buffer = m_view.TextBuffer;
            ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
            SnapshotPoint start = currentPos.BufferPosition;
            int lineNumber = currentSnapshot.GetLineNumberFromPosition(start.Position);
            for (int i = lineNumber + 1; i < currentSnapshot.LineCount; ++i)
            {
                ITextSnapshotLine line = currentSnapshot.GetLineFromLineNumber(i);
                string lineContents = line.GetTextIncludingLineBreak();
                if (string.IsNullOrWhiteSpace(lineContents))
                {
                    m_view.Caret.MoveTo(line.End);
                    break;
                }
            }
        }

        public void JumpBackward()
        {
            CaretPosition currentPos = m_view.Caret.Position;
            ITextBuffer buffer = m_view.TextBuffer;
            ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
            SnapshotPoint start = currentPos.BufferPosition;
            int lineNumber = currentSnapshot.GetLineNumberFromPosition(start.Position);
            for (int i = lineNumber - 1; i < currentSnapshot.LineCount; --i)
            {
                ITextSnapshotLine line = currentSnapshot.GetLineFromLineNumber(i);
                string lineContents = line.GetTextIncludingLineBreak();
                if (string.IsNullOrWhiteSpace(lineContents))
                {
                    m_view.Caret.MoveTo(line.End);
                    break;
                }
            }
        }
    }
}
