// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;

namespace Omnibar;

internal partial class BreadcrumbBarLayout : NonVirtualizingLayout
{
	// Fields

	private Size _availableSize;
	private BreadcrumbBarExItem? _ellipsisButton = null;
	private WeakReference? _breadcrumb = null;

	private bool _ellipsisIsRendered;
	private uint _firstRenderedItemIndexAfterEllipsis;
	private uint _visibleItemsCount;

	// Constructor

	public BreadcrumbBarLayout(BreadcrumbBarEx breadcrumb)
	{
		_breadcrumb = new WeakReference(breadcrumb);
	}

	// Override methods

	protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
	{
		_availableSize = availableSize;
		Size accumulatedCrumbsSize = new(0, 0);

		if (context.Children.Count is 0)
			return accumulatedCrumbsSize;

		for (int i = 0; i < context.Children.Count; i++)
		{
			var breadcrumbItem = (BreadcrumbBarExItem)context.Children[i];
			breadcrumbItem.Measure(availableSize);

			if (i is not -1)
			{
				accumulatedCrumbsSize.Width += breadcrumbItem.DesiredSize.Width;
				accumulatedCrumbsSize.Height = Math.Max(accumulatedCrumbsSize.Height, breadcrumbItem.DesiredSize.Height);
			}
		}

		// Get ellipsis item
		if (context.Children.Count > 0 && context.Children[0] is BreadcrumbBarExItem ellipsisButton)
			_ellipsisButton = ellipsisButton;

		// Available width is smaller than desired width
		if (accumulatedCrumbsSize.Width > availableSize.Width)
			_ellipsisIsRendered = accumulatedCrumbsSize.Width > availableSize.Width;

		return accumulatedCrumbsSize;
	}

	protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
	{
		if (context.Children.Count is 0)
			return new(0, 0);

		int itemCount = context.Children.Count;
		int indexOfFirstElementToRender = 0;
		_firstRenderedItemIndexAfterEllipsis = (uint)(itemCount - 1);
		_visibleItemsCount = 0;

		if (_ellipsisIsRendered)
		{
			indexOfFirstElementToRender = GetIndexOfFirstBreadcrumbBarItemToArrange(context);
			_firstRenderedItemIndexAfterEllipsis = (uint)indexOfFirstElementToRender;
		}

		float accumulatedWidths = 0f;
		float maxElementHeight = GetBreadcrumbBarItemsHeight(context, indexOfFirstElementToRender);

		// If there is at least one element, we may render the ellipsis item
		if (_ellipsisButton is not null)
		{
			Size elementSize = _ellipsisButton.DesiredSize;
			Rect arrangeRect = new(accumulatedWidths, 0, elementSize.Width, maxElementHeight);
			_ellipsisButton.Arrange(arrangeRect);

			accumulatedWidths += (float)elementSize.Width;

			//if (_ellipsisIsRendered)
			//{
			//	Size elementSize = _ellipsisButton.DesiredSize;
			//	Rect arrangeRect = new(accumulatedWidths, 0, elementSize.Width, maxElementHeight);
			//	_ellipsisButton.Arrange(arrangeRect);

			//	accumulatedWidths += (float)elementSize.Width;
			//}
			//else
			//{
			//	// Hide ellipsis item
			//	_ellipsisButton.Arrange(new(0, 0, 0, 0));
			//}
		}

		// For each item, if the item has an equal or larger index to the first element to render, then
		// render it, otherwise, hide it and add it to the list of hidden items
		for (int i = 1; i < itemCount; i++)
		{
			if (i < indexOfFirstElementToRender)
			{
				var element = context.Children[i];
				element.Arrange(new(0, 0, 0, 0));
			}
			else
			{
				var element = context.Children[i];
				Size elementSize = element.DesiredSize;
				Rect arrangeRect = new(accumulatedWidths, 0, elementSize.Width, maxElementHeight);
				element.Arrange(arrangeRect);

				accumulatedWidths += (float)elementSize.Width;

				_visibleItemsCount++;
			}
		}

		if (_breadcrumb?.Target is BreadcrumbBarEx breadcrumb)
			breadcrumb.ReIndexVisibleElementsForAccessibility();

		return finalSize;
	}

	internal bool EllipsisIsRendered()
	{
		return _ellipsisIsRendered;
	}

	internal uint FirstRenderedItemIndexAfterEllipsis()
	{
		return _firstRenderedItemIndexAfterEllipsis;
	}

	internal uint GetVisibleItemsCount()
	{
		return _visibleItemsCount;
	}

	// Private methods

	private int GetIndexOfFirstBreadcrumbBarItemToArrange(NonVirtualizingLayoutContext context)
	{
		int itemCount = context.Children.Count;
		float accumulatedLength = (float)context.Children[itemCount - 1].DesiredSize.Width + (float)(_ellipsisButton?.DesiredSize.Width ?? 0.0);

		for (int i = itemCount - 1; i >= 0; i--)
		{
			float newAccumulatedLength = accumulatedLength + (float)context.Children[i].DesiredSize.Width;
			if (newAccumulatedLength > _availableSize.Width)
				return i + 1;

			accumulatedLength = newAccumulatedLength;
		}

		return 0;
	}

	private float GetBreadcrumbBarItemsHeight(NonVirtualizingLayoutContext context, int indexOfFirstItemToRender)
	{
		float maxElementHeight = 0f;

		if (_ellipsisIsRendered)
			maxElementHeight = (float)(_ellipsisButton?.DesiredSize.Height ?? 0.0);

		for (int i = indexOfFirstItemToRender; i < context.Children.Count; i++)
			maxElementHeight = (float)Math.Max(maxElementHeight, context.Children[i].DesiredSize.Height);

		return maxElementHeight;
	}
}
