// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Omnibar;

public partial class BreadcrumbBarEx : Control
{
	private const string ItemsRepeaterPartName = "PART_ItemsRepeater";

	private ItemsSourceView? _breadcrumbItemsSourceView = null;
	private BreadcrumbBarEnumerable? _itemsEnumerable = null;
	private ItemsRepeater? _itemsRepeater = null;
	private BreadcrumbBarLayout? _itemsRepeaterLayout = null;
	private BreadcrumbBarExItem? _ellipsisBreadcrumbBarItem = null;
	private BreadcrumbBarExItem? _lastBreadcrumbBarItem = null;
	private int _focusedIndex = 1;

	public BreadcrumbBarEx()
	{
		DefaultStyleKey = typeof(BreadcrumbBarEx);

		_itemsRepeaterLayout = new(this);
		_itemsEnumerable = new();
	}

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		PreviewKeyDown += OnChildPreviewKeyDown;
		AccessKeyInvoked += OnAccessKeyInvoked;
		GettingFocus += OnGettingFocus;

		_itemsRepeater = (ItemsRepeater)GetTemplateChild(ItemsRepeaterPartName);

		if (_itemsRepeater is not null)
		{
			_itemsRepeater.Layout = _itemsRepeaterLayout;
			_itemsRepeater.ItemsSource = new ObservableCollection<object>();
			_itemsRepeater.ItemTemplate = ItemTemplate;

			// Unsubscribe
			_itemsRepeater.ElementPrepared -= OnElementPreparedEvent;
			_itemsRepeater.ElementIndexChanged -= OnElementIndexChangedEvent;
			_itemsRepeater.ElementClearing -= OnElementClearingEvent;
			_itemsRepeater.Loaded -= OnBreadcrumbBarItemsRepeaterLoaded;

			// Subscribe
			_itemsRepeater.ElementPrepared += OnElementPreparedEvent;
			_itemsRepeater.ElementIndexChanged += OnElementIndexChangedEvent;
			_itemsRepeater.ElementClearing += OnElementClearingEvent;
			_itemsRepeater.Loaded += OnBreadcrumbBarItemsRepeaterLoaded;
		}

		UpdateItemsRepeaterItemsSource();
	}

	internal ObservableCollection<object> GetHiddenItems()
	{
		if (_itemsRepeater is not null && _itemsRepeaterLayout is not null)
		{
			if (_itemsRepeaterLayout.EllipsisIsRendered())
			{
				var firstRenderedItemIndex = _itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis();

				var hiddenElements = new ObservableCollection<object>();

				if (_breadcrumbItemsSourceView != null)
					for (int i = 0; i < firstRenderedItemIndex - 1; ++i)
						hiddenElements.Add(_breadcrumbItemsSourceView.GetAt(i));

				return hiddenElements;
			}
		}

		return [];
	}

	internal void ReIndexVisibleElementsForAccessibility()
	{
		// Once the arrangement of BreadcrumbBar Items has happened then index all visible items
		if (_itemsRepeater is { } itemsRepeater && _itemsRepeaterLayout is not null)
		{
			uint visibleItemsCount = _itemsRepeaterLayout.GetVisibleItemsCount();
			var isEllipsisRendered = _itemsRepeaterLayout.EllipsisIsRendered();
			int firstItemToIndex = 1;

			if (isEllipsisRendered)
				firstItemToIndex = (int)_itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis();

			// In order to make the ellipsis inaccessible to accessibility tools when it's hidden,
			// we set the accessibilityView to raw and restore it to content when it becomes visible.
			if (_ellipsisBreadcrumbBarItem is { } ellipsisItem)
			{
				var accessibilityView = isEllipsisRendered ? AccessibilityView.Content : AccessibilityView.Raw;
				ellipsisItem.SetValue(AutomationProperties.AccessibilityViewProperty, accessibilityView);
			}

			var itemsSourceView = itemsRepeater.ItemsSourceView;

			// For every BreadcrumbBar item we set the index (starting from 1 for the root/highest-level item)
			// accessibilityIndex is the index to be assigned to each item
			// itemToIndex is the real index and it may differ from accessibilityIndex as we must only index the visible items
			for (int accessibilityIndex = 1, itemToIndex = firstItemToIndex; accessibilityIndex <= visibleItemsCount; ++accessibilityIndex, ++itemToIndex)
			{
				if (itemsRepeater.TryGetElement(itemToIndex) is { } element)
				{
					element.SetValue(AutomationProperties.PositionInSetProperty, accessibilityIndex);
					element.SetValue(AutomationProperties.SizeOfSetProperty, visibleItemsCount);
				}
			}
		}
	}
}
