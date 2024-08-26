// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Omnibar;

public partial class BreadcrumbBarEx
{
	private void OnFlowDirectionChanged(DependencyObject sender, DependencyProperty property)
	{
		// Only if some ItemsSource has been defined then we change the BreadcrumbBarItems flow direction
		if (ItemsSource != null && _itemsRepeater is { } itemsRepeater)
		{
			// Add 1 to account for the leading null
			for (int i = 0; i < (_breadcrumbItemsSourceView?.Count ?? 0) + 1; ++i)
			{
				var element = itemsRepeater.TryGetElement(i) as BreadcrumbBarExItem;
				if (element != null)
					element.FlowDirection = FlowDirection;
			}
		}
	}

	private void OnBreadcrumbBarItemsSourceCollectionChanged(object? sender, object? args)
	{
		if (_itemsRepeater is { } itemsRepeater)
		{
			// A new BreadcrumbEnumerable must be created as ItemsRepeater compares if the previous
			// itemsSource is equals to the new one
			_itemsEnumerable = new(ItemsSource);
			itemsRepeater.ItemsSource = _itemsEnumerable;
		}
	}

	private void OnBreadcrumbBarItemsRepeaterLoaded(object sender, RoutedEventArgs property)
	{
		if (_itemsRepeater is { } breadcrumbItemsRepeater)
			OnBreadcrumbBarItemsSourceCollectionChanged(null, null);
	}

	private void OnElementPreparedEvent(ItemsRepeater itemsRepeater, ItemsRepeaterElementPreparedEventArgs args)
	{
		if (args.Element is BreadcrumbBarExItem item)
		{
			item.SetIsEllipsisDropDownItem(false);

			// Set the parent breadcrumb reference for raising click events
			item.SetParentBreadcrumbBar(this);

			// Set the item index to fill the Index parameter in the ClickedEventArgs
			int itemIndex = args.Index;
			item.SetIndex(itemIndex);

			// The first element is always the ellipsis item
			if (itemIndex == 0)
			{
				item.SetPropertiesForEllipsisItem();
				_ellipsisBreadcrumbBarItem = item;
				UpdateEllipsisItemTemplate();

				AutomationProperties.SetName(item, "AutomationNameEllipsisBreadcrumbBarItem");
			}
			else
			{
				if (_breadcrumbItemsSourceView != null)
				{
					// Any other element just resets the visual properties
					item.ResetVisualProperties();
				}
			}
		}
	}

	private void OnElementIndexChangedEvent(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
	{
		if (_focusedIndex == args.OldIndex)
		{
			var newIndex = args.NewIndex;

			if (args.Element is BreadcrumbBarExItem item)
				item.SetIndex(newIndex);

			FocusElementAt(newIndex);
		}
	}

	private void OnElementClearingEvent(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
	{
		if (args.Element is BreadcrumbBarExItem item)
			item.ResetVisualProperties();
	}

	// When focus comes from outside the BreadcrumbBar control we will put focus on the selected item.
	private void OnGettingFocus(object sender, GettingFocusEventArgs args)
	{
		if (_itemsRepeater is { } itemsRepeater)
		{
			var inputDevice = args.InputDevice;
			if (inputDevice == FocusInputDeviceKind.Keyboard)
			{
				// If focus is coming from outside the repeater, put focus on the selected item.
				var oldFocusedElement = args.OldFocusedElement;
				if (oldFocusedElement == null || itemsRepeater != VisualTreeHelper.GetParent(oldFocusedElement))
				{
					// Reset the focused index
					if (_itemsRepeaterLayout != null)
					{
						_focusedIndex = _itemsRepeaterLayout.EllipsisIsRendered() ? 0 : 1;
						FocusElementAt(_focusedIndex);
					}

					if (itemsRepeater.TryGetElement(_focusedIndex) is { } selectedItem)
					{
						if (args.TrySetNewFocusedElement(selectedItem))
							args.Handled = true;
					}
				}

				// Focus was already in the repeater: in RS3+ Selection follows focus unless control is held down.
				else if ((InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down)
				{
					if (args.NewFocusedElement is UIElement newFocusedElementAsUIE)
					{
						FocusElementAt(itemsRepeater.GetElementIndex(newFocusedElementAsUIE));
						args.Handled = true;
					}
				}
			}
		}
	}

	private void OnChildPreviewKeyDown(object sender, KeyRoutedEventArgs args)
	{
		bool flowDirectionIsLTR = (FlowDirection == FlowDirection.LeftToRight);
		bool keyIsLeft = (args.Key == VirtualKey.Left);
		bool keyIsRight = (args.Key == VirtualKey.Right);

		// Moving to the next element
		if ((flowDirectionIsLTR && keyIsRight) || (!flowDirectionIsLTR && keyIsLeft))
		{
			if (MoveFocusNext())
			{
				args.Handled = true;
				return;
			}
			else if ((flowDirectionIsLTR && (args.OriginalKey == VirtualKey.GamepadDPadRight)) ||
				(!flowDirectionIsLTR && (args.OriginalKey == VirtualKey.GamepadDPadLeft)))
			{
				var options = new FindNextElementOptions();
				options.SearchRoot = XamlRoot?.Content;

				if (FocusManager.TryMoveFocus(FocusNavigationDirection.Next, options))
				{
					args.Handled = true;
					return;
				}
			}
		}
		// Moving to previous element
		else if ((flowDirectionIsLTR && keyIsLeft) || (!flowDirectionIsLTR && keyIsRight))
		{
			if (MoveFocusPrevious())
			{
				args.Handled = true;
				return;
			}
			else if ((flowDirectionIsLTR && (args.OriginalKey == VirtualKey.GamepadDPadLeft)) ||
						(!flowDirectionIsLTR && (args.OriginalKey == VirtualKey.GamepadDPadRight)))
			{
				var options = new FindNextElementOptions();
				options.SearchRoot = XamlRoot?.Content;
				if (FocusManager.TryMoveFocus(FocusNavigationDirection.Previous, options))
				{
					args.Handled = true;
					return;
				}
			}
		}
	}

	private void OnAccessKeyInvoked(UIElement sender, AccessKeyInvokedEventArgs args)
	{
		// If BreadcrumbBar is an AccessKeyScope then we do not want to handle the access
		// key invoked event because the user has (probably) set up access keys for the
		// BreadcrumbBarItem elements.
		if (!IsAccessKeyScope)
		{
			if (_focusedIndex > 0)
			{
				if (_itemsRepeater is { } itemsRepeater)
				{
					if (itemsRepeater.TryGetElement(_focusedIndex) is { } selectedItem)
					{
						if (selectedItem is Control selectedItemAsControl)
						{
							args.Handled = selectedItemAsControl.Focus(FocusState.Programmatic);
							return;
						}
					}
				}
			}

			// If we don't have a selected index, focus the RadioButton's which under normal
			// circumstances will put focus on the first radio button.
			args.Handled = Focus(FocusState.Programmatic);
		}
	}

	private void FocusElementAt(int index)
	{
		if (index >= 0)
			_focusedIndex = index;
	}

	private bool MoveFocus(int indexIncrement)
	{
		if (_itemsRepeater is not null)
		{
			var focusedElem = XamlRoot is null
				? FocusManager.GetFocusedElement()
				: FocusManager.GetFocusedElement(XamlRoot);

			if (focusedElem is UIElement focusedElement)
			{
				var focusedIndex = _itemsRepeater.GetElementIndex(focusedElement);
				if (focusedIndex >= 0 && indexIncrement != 0)
				{
					focusedIndex += indexIncrement;
					var itemCount = _itemsRepeater.ItemsSourceView.Count;

					while (focusedIndex >= 0 && focusedIndex < itemCount)
					{
						if (_itemsRepeater.TryGetElement(focusedIndex) is { } item)
						{
							if (item is UIElement itemAsUIE)
							{
								if (itemAsUIE.Focus(FocusState.Programmatic))
								{
									FocusElementAt(focusedIndex);
									return true;
								}
							}
						}

						focusedIndex += indexIncrement;
					}
				}
			}
		}

		return false;
	}

	private bool MoveFocusPrevious()
	{
		int movementPrevious = -1;

		// If the focus is in the first visible item, then move to the ellipsis
		if (_itemsRepeater is not null)
		{
			if (_itemsRepeaterLayout != null)
			{
				if (_focusedIndex == 1)
					movementPrevious = 0;
				else if (_itemsRepeaterLayout.EllipsisIsRendered() &&
					_focusedIndex == (int)_itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis())
					movementPrevious = -_focusedIndex;
			}
		}

		return MoveFocus(movementPrevious);
	}

	private bool MoveFocusNext()
	{
		int movementNext = 1;

		// If the focus is in the ellipsis, then move to the first visible item 
		if (_focusedIndex == 0)
		{
			if (_itemsRepeater is not null &&
				_itemsRepeaterLayout != null)
				movementNext = (int)_itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis();
		}

		return MoveFocus(movementNext);
	}

	internal void OnItemClicked(object content, int index)
	{
		var eventArgs = new BreadcrumbBarItemClickedEventArgs(content, index);

		ItemClicked?.Invoke(this, eventArgs);
	}
}
