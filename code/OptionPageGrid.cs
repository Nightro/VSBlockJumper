using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using Microsoft.VisualStudio.Text.Editor;

namespace VSBlockJumper
{
	public enum CollapsedRegionHandling
	{
		Skip,
		ExpandIfContainsBlockEdge,
		ExpandAlways,
	}

	public class OptionPageGrid : DialogPage
	{
		public static IEditorOptionsFactoryService OptionsService { get; set; }

		[Category("General")]
		[DisplayName("Jump Outside Edge")]
		[Description("If enabled the cursor will jump outside of the block edge (blank line), otherwise it jumps inside the block edge (text line).")]
		public bool JumpOutsideEdge
		{
			get
			{
				return OptionsService.GlobalOptions.GetOptionValue(JumpOutsideEdgeOption.OptionKey);
			}

			set
			{
				OptionsService.GlobalOptions.SetOptionValue(JumpOutsideEdgeOption.OptionKey, value);
			}
		}

		[Category("General")]
		[DisplayName("Skip Closest Edge")]
		[Description("If enabled, the cursor will only jump to the far edge of a block, otherwise it visits every edge of a block.")]
		public bool SkipClosestEdge
		{
			get
			{
				return OptionsService.GlobalOptions.GetOptionValue(SkipClosestEdgeOption.OptionKey);
			}

			set
			{
				OptionsService.GlobalOptions.SetOptionValue(SkipClosestEdgeOption.OptionKey, value);
			}
		}

		[Category("General")]
		[DisplayName("Collapsed Regions")]
		[Description("How should collapsed regions be handled?")]
		public CollapsedRegionHandling CollapsedRegionHandling
		{
			get
			{
				return OptionsService.GlobalOptions.GetOptionValue<CollapsedRegionHandling>(CollapsedRegionHandlingOption.OptionKey);
			}

			set
			{
				OptionsService.GlobalOptions.SetOptionValue(CollapsedRegionHandlingOption.OptionKey, value);
			}
		}
	}
}
