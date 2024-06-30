//------------------------------------------------------------------------------
// <copyright file="CommandFilter.cs" company="OzmosisGames">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
// <date>27/03/17</date>
// <author>Anthony Reddan</author>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Outlining;
using System.Collections.Generic;

namespace VSBlockJumper
{
	[Export(typeof(EditorOptionDefinition))]
	[Name(OptionName)]
	public sealed class JumpOutsideEdgeOption : WpfViewOptionDefinition<bool>
	{
		public const string OptionName = "VSBlockJumper/JumpOutsideEdge";
		public readonly static EditorOptionKey<bool> OptionKey = new EditorOptionKey<bool>(OptionName);

		public override bool Default { get { return true; } }
		public override EditorOptionKey<bool> Key { get { return OptionKey; } }
	}

	[Export(typeof(EditorOptionDefinition))]
	[Name(OptionName)]
	public sealed class SkipClosestEdgeOption : WpfViewOptionDefinition<bool>
	{
		public const string OptionName = "VSBlockJumper/SkipClosestEdge";
		public readonly static EditorOptionKey<bool> OptionKey = new EditorOptionKey<bool>(OptionName);

		public override bool Default { get { return false; } }
		public override EditorOptionKey<bool> Key { get { return OptionKey; } }
	}

	[Export(typeof(EditorOptionDefinition))]
	[Name(OptionName)]
	public sealed class CollapsedRegionHandlingOption : WpfViewOptionDefinition<CollapsedRegionHandling>
	{
		public const string OptionName = "VSBlockJumper/ExpandCollpasedRegions";
		public readonly static EditorOptionKey<CollapsedRegionHandling> OptionKey = new EditorOptionKey<CollapsedRegionHandling>(OptionName);

		public override CollapsedRegionHandling Default { get { return CollapsedRegionHandling.Skip; } }
		public override EditorOptionKey<CollapsedRegionHandling> Key { get { return OptionKey; } }
	}

	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Interactive)]
	internal class TextViewCreationListener : IWpfTextViewCreationListener
	{
		[Import(typeof(IVsEditorAdaptersFactoryService))]
		private IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

		[Import(typeof(ISmartIndentationService))]
		private ISmartIndentationService SmartIndentationService { get; set; }

		[Import(typeof(IOutliningManagerService))]
		private IOutliningManagerService OutliningManagerService { get; set; }

		public void TextViewCreated(IWpfTextView textView)
		{
			IOutliningManager outliningManager = OutliningManagerService.GetOutliningManager(textView);
			CommandFilter filter = new CommandFilter(textView, SmartIndentationService, outliningManager);
			IVsTextView view = EditorAdaptersFactoryService.GetViewAdapter(textView);

			if (view != null)
			{
				int result = view.AddCommandFilter(filter, out IOleCommandTarget next);
				if (result == VSConstants.S_OK)
				{
					filter.Next = next;
				}
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

		private bool JumpOutsideEdge
		{
			get
			{
				return View.Options.GetOptionValue(JumpOutsideEdgeOption.OptionKey);
			}
		}

		private bool SkipClosestEdge
		{
			get
			{
				return View.Options.GetOptionValue(SkipClosestEdgeOption.OptionKey);
			}
		}

		private CollapsedRegionHandling CollapsedRegionHandling
		{
			get
			{
				return View.Options.GetOptionValue(CollapsedRegionHandlingOption.OptionKey);
			}
		}

		private IWpfTextView View { get; }
		private ISmartIndentationService SmartIndentationService { get; }
		private IOutliningManager OutliningManager { get; }
		public IOleCommandTarget Next { get; set; }

		public CommandFilter(IWpfTextView view, ISmartIndentationService smartIndentationService, IOutliningManager outliningManager)
		{
			View = view;
			SmartIndentationService = smartIndentationService;
			OutliningManager = outliningManager;
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

			// rules:
			// if the line we begin on contains text, or the following line contains text, 
			// navigate to the next blank line
			// if the line we begin on is blank, and the next line is also blank, 
			// navigate to the line preceding the next line that contains text
			// if we find no suitable lines, we navigate to the last line
			CaretPosition startingPos = View.Caret.Position;
			ITextBuffer buffer = View.TextBuffer;
			ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
			SnapshotPoint start = startingPos.BufferPosition;

			// collect collapsed regious so we can skip them
			SnapshotSpan regionSearchArea;
			if (direction == JumpDirection.Up)
			{
				regionSearchArea = new SnapshotSpan(currentSnapshot, 0, start);
			}
			else
			{
				regionSearchArea = new SnapshotSpan(currentSnapshot, start, currentSnapshot.Length - start);
			}

			// pre-collect the line numbers so we can just check those in the loop
			IEnumerable<ICollapsed> collapsedRegions = OutliningManager.GetCollapsedRegions(regionSearchArea);
			List<(ICollapsed, int, int)> collapsedRegionLineSpans = new List<(ICollapsed, int, int)>(32);
			foreach (ICollapsed collapsedRegion in collapsedRegions)
			{
				SnapshotSpan regionSpan = collapsedRegion.Extent.GetSpan(currentSnapshot);
				collapsedRegionLineSpans.Add((collapsedRegion, regionSpan.Start.GetContainingLineNumber(), regionSpan.End.GetContainingLineNumber()));
			}

			// regions are already sorted linearly for jump down, just reverse for up
			if (direction == JumpDirection.Up)
			{
				collapsedRegionLineSpans.Reverse();
			}

			ITextSnapshotLine previousLine = start.GetContainingLine();
			ITextSnapshotLine targetLine = null;
			bool previousLineIsBlank = string.IsNullOrWhiteSpace(previousLine.GetTextIncludingLineBreak());
			int startLineNo = currentSnapshot.GetLineNumberFromPosition(start.Position);
			int lineInc = (int)direction;
			int firstLine = startLineNo + lineInc;
			for (int i = firstLine; i >= 0 && i < currentSnapshot.LineCount; i += lineInc)
			{
				if (CollapsedRegionHandling != CollapsedRegionHandling.ExpandIfContainsBlockEdge)
				{
					for (int j = 0; j < collapsedRegionLineSpans.Count; ++j)
					{
						(ICollapsed collapsedRegion, int regionStartLine, int regionEndLine) = collapsedRegionLineSpans[j];
						if (i > regionStartLine && i <= regionEndLine)
						{
							if (CollapsedRegionHandling == CollapsedRegionHandling.ExpandAlways)
							{
								// if we expand collapsed regions, remove it, expand, and continue on as normal
								collapsedRegionLineSpans.RemoveAt(j);
								OutliningManager.Expand(collapsedRegion);
								break;
							}

							// skip to the line at the end of the region and go next
							int nextLine = regionStartLine;
							if (direction == JumpDirection.Down)
							{
								nextLine = regionEndLine + 1;
							}

							// this is kind of an annoying edge case to handle
							if (i == firstLine)
							{
								firstLine = nextLine;
							}

							i = nextLine;
						}
					}
				}

				ITextSnapshotLine line = currentSnapshot.GetLineFromLineNumber(i);
				string lineContents = line.GetTextIncludingLineBreak();
				bool lineIsBlank = string.IsNullOrWhiteSpace(lineContents);
				if (lineIsBlank)
				{
					if (!previousLineIsBlank && i != firstLine)
					{
						// found our next blank line beyond our text block
						targetLine = (JumpOutsideEdge) ? line : previousLine;
						break;
					}
				}
				else if (!SkipClosestEdge && previousLineIsBlank && i != firstLine)
				{
					// found our text block, go to the blank line right before it
					targetLine = (JumpOutsideEdge) ? previousLine : line;
					break;
				}

				previousLine = line;
				previousLineIsBlank = lineIsBlank;
			}

			if (targetLine != null)
			{
				VirtualSnapshotPoint finalPosition;
				if (JumpOutsideEdge)
				{
					// move the caret to the blank line indented with the appropriate number of virtual spaces
					int? virtualSpaces = SmartIndentationService.GetDesiredIndentation(View, targetLine);
					finalPosition = new VirtualSnapshotPoint(targetLine.Start, virtualSpaces.GetValueOrDefault());
					if (!finalPosition.IsInVirtualSpace)
					{
						// our line has some 'meaningful' whitespace, go to end instead
						finalPosition = new VirtualSnapshotPoint(targetLine.End);
					}
				}
				else
				{
					string lineString = targetLine.GetTextIncludingLineBreak();
					int offset = lineString.TakeWhile(c => char.IsWhiteSpace(c)).Count();
					finalPosition = new VirtualSnapshotPoint(targetLine, offset);
				}

				// if our block edge resides inside a region - expand it
				if (CollapsedRegionHandling == CollapsedRegionHandling.ExpandIfContainsBlockEdge)
				{
					for (int j = 0; j < collapsedRegionLineSpans.Count; ++j)
					{
						(ICollapsed collapsedRegion, int regionStartLine, int regionEndLine) = collapsedRegionLineSpans[j];
						if (targetLine.LineNumber > regionStartLine && targetLine.LineNumber <= regionEndLine)
						{
							OutliningManager.Expand(collapsedRegion);
						}
					}
				}

				View.Caret.MoveTo(finalPosition);
			}
			else
			{
				// we found no suitable line so just choose BOF or EOF depending on the direction we were moving
				if (direction == JumpDirection.Up)
				{
					View.Caret.MoveTo(previousLine.Start);
				}
				else
				{
					View.Caret.MoveTo(previousLine.End);
				}
			}

			// scroll our view to the new caret position
			View.Caret.EnsureVisible();
		}
	}
}
