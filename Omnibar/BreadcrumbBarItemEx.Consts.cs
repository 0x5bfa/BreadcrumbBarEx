// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Omnibar
{
	public partial class BreadcrumbBarItemEx
	{
		// Common Visual States
		private const string NormalStateName = "Normal";
		private const string CurrentStateName = "Current";
		private const string PointerOverStateName = "PointerOver";
		private const string PressedStateName = "Pressed";
		private const string DisabledStateName = "Disabled";

		// Inline Item Type Visual States
		private const string EllipsisStateName = "Ellipsis";
		private const string EllipsisRTLStateName = "EllipsisRTL";
		private const string LastItemStateName = "LastItem";
		private const string DefaultStateName = "Default";
		private const string DefaultRTLStateName = "DefaultRTL";

		// Item Type Visual States
		private const string InlineStateName = "Inline";
		private const string EllipsisDropDownStateName = "EllipsisDropDown";

		// Template Parts
		private const string EllipsisItemsRepeaterPartName = "PART_EllipsisItemsRepeater";
		private const string ItemButtonPartName = "PART_ItemButton";
		private const string ItemEllipsisFlyoutPartName = "PART_EllipsisFlyout";

		// Automation Names
		private const string EllipsisFlyoutAutomationName = "EllipsisFlyout";
		private const string EllipsisItemsRepeaterAutomationName = "EllipsisItemsRepeater";
	}
}
