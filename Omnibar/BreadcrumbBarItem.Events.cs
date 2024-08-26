// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using System.Diagnostics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Omnibar;

public partial class BreadcrumbBarExItem
{
	// Pointer event methods

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new BreadcrumbBarItemAutomationPeer(this);
	}

	protected override void OnPointerReleased(PointerRoutedEventArgs args)
	{
		base.OnPointerReleased(args);

		if (_isEllipsisDropDownItem)
		{
			if (IgnorePointerId(args))
				return;

			if (_isPressed)
			{
				_isPressed = false;
				UpdateEllipsisDropDownItemVisualState(true);
				OnClick(null, null);
			}
		}
	}

	protected override void OnPointerCanceled(PointerRoutedEventArgs args)
	{
		base.OnPointerCanceled(args);

		if (_isEllipsisDropDownItem)
			ProcessPointerCanceled(args);
	}

	protected override void OnPointerCaptureLost(PointerRoutedEventArgs args)
	{
		base.OnPointerCaptureLost(args);

		if (_isEllipsisDropDownItem)
			ProcessPointerCanceled(args);
	}

	protected override void OnPointerEntered(PointerRoutedEventArgs args)
	{
		base.OnPointerEntered(args);

		if (_isEllipsisDropDownItem)
			ProcessPointerOver(args);
	}

	protected override void OnPointerMoved(PointerRoutedEventArgs args)
	{
		base.OnPointerMoved(args);

		if (_isEllipsisDropDownItem)
			ProcessPointerOver(args);
	}

	protected override void OnPointerExited(PointerRoutedEventArgs args)
	{
		base.OnPointerExited(args);

		if (_isEllipsisDropDownItem)
			ProcessPointerCanceled(args);
	}

	protected override void OnPointerPressed(PointerRoutedEventArgs args)
	{
		base.OnPointerPressed(args);

		if (_isEllipsisDropDownItem)
		{
			if (IgnorePointerId(args))
				return;

			Debug.Assert(!_isPressed);

			if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				var pointerProperties = args.GetCurrentPoint(this).Properties;
				_isPressed = pointerProperties.IsLeftButtonPressed;
			}
			else
			{
				_isPressed = true;
			}

			if (_isPressed)
				UpdateEllipsisDropDownItemVisualState(true);
		}
	}

	private void ProcessPointerOver(PointerRoutedEventArgs args)
	{
		Debug.Assert(_isEllipsisDropDownItem);

		if (IgnorePointerId(args))
		{
			return;
		}

		if (!_isPointerOver)
		{
			_isPointerOver = true;
			UpdateEllipsisDropDownItemVisualState(true);
		}
	}

	private void ProcessPointerCanceled(PointerRoutedEventArgs args)
	{
		Debug.Assert(_isEllipsisDropDownItem);

		if (IgnorePointerId(args))
		{
			return;
		}

		_isPressed = false;
		_isPointerOver = false;

		Debug.Assert(_isEllipsisDropDownItem);

		_trackedPointerId = 0;

		UpdateEllipsisDropDownItemVisualState(true);
	}

	private bool IgnorePointerId(PointerRoutedEventArgs args)
	{
		// Returns False when the provided pointer Id matches the currently tracked Id.
		// When there is no currently tracked Id, sets the tracked Id to the provided Id and returns False.
		// Returns True when the provided pointer Id does not match the currently tracked Id.

		Debug.Assert(_isEllipsisDropDownItem);

		uint pointerId = args.Pointer.PointerId;

		if (_trackedPointerId == 0)
		{
			_trackedPointerId = pointerId;
		}
		else if (_trackedPointerId != pointerId)
		{
			return true;
		}

		return false;
	}

	// Click event methods

	internal void OnClick(object? sender, RoutedEventArgs? args)
	{
		if (_isEllipsisDropDownItem)
		{
			if (_ellipsisItem is { } ellipsisItem)
			{
				// Once an element has been clicked, close the flyout
				ellipsisItem.CloseFlyout();
				ellipsisItem.RaiseItemClickedEvent(Content, _index - 1);
			}
		}
		else if (_isEllipsisItem)
		{
			OnEllipsisItemClick(null, null);
		}
		else
		{
			OnBreadcrumbBarItemClick(null, null);
		}
	}

	private void OnBreadcrumbBarItemClick(object? sender, RoutedEventArgs? args)
	{
		RaiseItemClickedEvent(Content, _index - 1);
	}

	private void OnEllipsisItemClick(object? sender, RoutedEventArgs? args)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (_parentBreadcrumb?.Target is BreadcrumbBarEx breadcrumb)
		{
			var hiddenElements = CloneEllipsisItemSource(breadcrumb.GetHiddenItems());

			if (_ellipsisItemsRepeater is { } flyoutRepeater)
				flyoutRepeater.ItemsSource = hiddenElements;

			OpenFlyout();
		}
	}

	private void RaiseItemClickedEvent(object content, int index)
	{
		if (_parentBreadcrumb?.Target is BreadcrumbBarEx breadcrumb)
			breadcrumb.OnItemClicked(content, index);
	}

	// Flyout event methods

	private void OnFlyoutElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (args.Element is BreadcrumbBarExItem ellipsisDropDownItem &&
			ellipsisDropDownItem is { } ellipsisDropDownItemImpl)
			ellipsisDropDownItemImpl.SetIsEllipsisDropDownItem(true);

		UpdateFlyoutIndex(args.Element, args.Index);
	}

	private void OnFlyoutElementIndexChanged(ItemsRepeater itemsRepeater, ItemsRepeaterElementIndexChangedEventArgs args)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		UpdateFlyoutIndex(args.Element, args.NewIndex);
	}

	// Misc event methods

	private void OnLoaded(object sender, RoutedEventArgs args)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (_itemButton is { } button)
		{
			if (_isEllipsisItem)
			{
				button.Click -= OnEllipsisItemClick;
				button.Click += OnEllipsisItemClick;
			}
			else
			{
				button.Click -= OnBreadcrumbBarItemClick;
				button.Click += OnBreadcrumbBarItemClick;
			}
		}

		if (_isEllipsisItem)
			SetPropertiesForEllipsisItem();
		else if (_isLastItem)
			SetPropertiesForLastItem();
		else
			ResetVisualProperties();
	}

	private void OnFlowDirectionChanged(DependencyObject sender, DependencyProperty property)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		UpdateInlineItemTypeVisualState(true);
	}

	private void OnChildPreviewKeyDown(object sender, KeyRoutedEventArgs args)
	{
		if (_isEllipsisDropDownItem)
		{
			if (args.Key == VirtualKey.Enter || args.Key == VirtualKey.Space)
			{
				OnClick(sender, null);
				args.Handled = true;
			}
		}
		else if (args.Key == VirtualKey.Enter || args.Key == VirtualKey.Space)
		{
			if (_isEllipsisItem)
				OnEllipsisItemClick(null, null);
			else
				OnBreadcrumbBarItemClick(null, null);

			args.Handled = true;
		}
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
	{
		Debug.Assert(_isEllipsisDropDownItem);

		UpdateEllipsisDropDownItemVisualState(true);
	}

	private void OnVisualPropertyChanged(DependencyObject sender, DependencyProperty property)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		UpdateButtonVisualState(true);
	}
}
