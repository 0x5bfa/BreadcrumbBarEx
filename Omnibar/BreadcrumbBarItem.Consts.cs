// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Omnibar;

public partial class BreadcrumbBarExItem
{
	// Common Visual States
	private const string _normalStateName = "Normal";
	private const string _currentStateName = "Current";
	private const string _pointerOverStateName = "PointerOver";
	private const string _pressedStateName = "Pressed";
	private const string _disabledStateName = "Disabled";

	// Inline Item Type Visual States
	private const string _ellipsisStateName = "Ellipsis";
	private const string _ellipsisRTLStateName = "EllipsisRTL";
	private const string _lastItemStateName = "LastItem";
	private const string _defaultStateName = "Default";
	private const string _defaultRTLStateName = "DefaultRTL";

	// Item Type Visual States
	private const string _inlineStateName = "Inline";
	private const string _ellipsisDropDownStateName = "EllipsisDropDown";

	// Template Parts
	private const string _ellipsisItemsRepeaterPartName = "PART_EllipsisItemsRepeater";
	private const string _itemButtonPartName = "PART_ItemButton";
	private const string _itemEllipsisFlyoutPartName = "PART_EllipsisFlyout";

	// Automation Names
	private const string _ellipsisFlyoutAutomationName = "EllipsisFlyout";
	private const string _ellipsisItemsRepeaterAutomationName = "EllipsisItemsRepeater";
}
