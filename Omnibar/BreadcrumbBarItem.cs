// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Omnibar;

public partial class BreadcrumbBarExItem : ContentControl
{
	// Fields

	protected bool _fullyInitialized = false;
	private int _index;
	private bool _isEllipsisDropDownItem;
	private bool _isEllipsisItem;
	private bool _isLastItem;

	private Button? _itemButton = null;
	private WeakReference? _parentBreadcrumb = null;
	private Flyout? _ellipsisFlyout = null;
	private ItemsRepeater? _ellipsisItemsRepeater = null;
	private BreadcrumbBarExItem? _ellipsisItem = null;
	private object? _itemTemplate = null;

	private uint _trackedPointerId = 0;
	private bool _isPressed = false;
	private bool _isPointerOver = false;
	private long? _flowDirectionChangedToken = null;

	// Initializers

	public BreadcrumbBarExItem()
	{
		DefaultStyleKey = typeof(BreadcrumbBarExItem);
	}

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		if (_isEllipsisDropDownItem)
		{
			UpdateEllipsisDropDownItemVisualState(false);
		}
		else
		{
			_itemButton = GetTemplateChild(_itemButtonPartName) as Button;

			if (_itemButton is { } button)
			{
				button.Loaded -= OnLoaded;
				button.Loaded += OnLoaded;

				RegisterPropertyChangedCallback(ButtonBase.IsPressedProperty, OnVisualPropertyChanged);
				RegisterPropertyChangedCallback(ButtonBase.IsPointerOverProperty, OnVisualPropertyChanged);
				RegisterPropertyChangedCallback(IsEnabledProperty, OnVisualPropertyChanged);
			}

			UpdateButtonVisualState(false);
			UpdateInlineItemTypeVisualState(false);
		}

		UpdateItemTypeVisualState();

		_fullyInitialized = true;
	}

	// Methods

	internal void SetParentBreadcrumbBar(BreadcrumbBarEx parent)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		_parentBreadcrumb = new(parent);
	}

	internal void SetIndex(int index)
	{
		_index = index;
	}

	internal void SetDataTemplate(object? newDataTemplate)
	{
		_itemTemplate = newDataTemplate;
	}

	internal void SetIsEllipsisDropDownItem(bool isEllipsisDropDownItem)
	{
		_isEllipsisDropDownItem = isEllipsisDropDownItem;

		PreviewKeyDown -= OnChildPreviewKeyDown;
		PreviewKeyDown += OnChildPreviewKeyDown;

		if (_isEllipsisDropDownItem)
		{
			IsEnabledChanged -= OnIsEnabledChanged;
			IsEnabledChanged += OnIsEnabledChanged;
		}
		else
		{
			_flowDirectionChangedToken ??= RegisterPropertyChangedCallback(FlowDirectionProperty, OnFlowDirectionChanged);
		}

		UpdateItemTypeVisualState();
	}

	internal void SetPropertiesForLastItem()
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		_isEllipsisItem = false;
		_isLastItem = true;

		UpdateButtonVisualState(false);
		UpdateInlineItemTypeVisualState(false);
	}

	internal void ResetVisualProperties()
	{
		if (_isEllipsisDropDownItem)
		{
			UpdateEllipsisDropDownItemVisualState(false);
		}
		else
		{
			_isEllipsisItem = false;
			_isLastItem = false;

			if (_itemButton is { } button)
				button.Flyout = null;

			_ellipsisFlyout = null;
			_ellipsisItemsRepeater = null;

			UpdateButtonVisualState(false);
			UpdateInlineItemTypeVisualState(false);
		}
	}

	internal void SetPropertiesForEllipsisItem()
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		_isEllipsisItem = true;
		_isLastItem = false;

		InstantiateFlyout();

		UpdateButtonVisualState(false);
		UpdateInlineItemTypeVisualState(false);
	}

	// Private methods

	private void InstantiateFlyout()
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		// Only if the element has been created visually, instantiate the flyout
		if (_itemButton is { } button &&
			_ellipsisFlyout is { } ellipsisFlyout)
		{
			// Create ItemsRepeater and set the DataTemplate 
			var ellipsisItemsRepeater = new ItemsRepeater();
			ellipsisItemsRepeater.Name = _ellipsisItemsRepeaterPartName;
			AutomationProperties.SetName(ellipsisItemsRepeater, _ellipsisItemsRepeaterAutomationName);
			ellipsisItemsRepeater.HorizontalAlignment = HorizontalAlignment.Stretch;

			ellipsisItemsRepeater.Layout = new StackLayout();

			ellipsisItemsRepeater.ItemTemplate = _itemTemplate;

			ellipsisItemsRepeater.ElementPrepared -= OnFlyoutElementPrepared;
			ellipsisItemsRepeater.ElementPrepared += OnFlyoutElementPrepared;

			ellipsisItemsRepeater.ElementIndexChanged -= OnFlyoutElementIndexChanged;
			ellipsisItemsRepeater.ElementIndexChanged += OnFlyoutElementIndexChanged;

			_ellipsisItemsRepeater = ellipsisItemsRepeater;

			// Set the repeater as the content.
			AutomationProperties.SetName(ellipsisFlyout, _ellipsisFlyoutAutomationName);
			ellipsisFlyout.Content = ellipsisItemsRepeater;
			ellipsisFlyout.Placement = FlyoutPlacementMode.Bottom;

			_ellipsisFlyout = ellipsisFlyout;
		}
	}

	private void OpenFlyout()
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (_ellipsisFlyout is { } flyout)
			flyout.ShowAt(this, new());
	}

	private void CloseFlyout()
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (_ellipsisFlyout is { } flyout)
			flyout.Hide();
	}

	private void UpdateFlyoutIndex(UIElement element, int index)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (_ellipsisItemsRepeater is { } ellipsisItemsRepeater)
		{
			if (ellipsisItemsRepeater.ItemsSourceView is { } itemSourceView)
			{
				int itemCount = itemSourceView.Count;

				if (element is BreadcrumbBarExItem ellipsisDropDownItem)
				{
					Debug.Assert(ellipsisDropDownItem._isEllipsisDropDownItem);
					ellipsisDropDownItem._ellipsisItem = this;
					ellipsisDropDownItem.SetIndex(itemCount - index);
				}

				element.SetValue(AutomationProperties.PositionInSetProperty, index + 1);
				element.SetValue(AutomationProperties.SizeOfSetProperty, itemCount);
			}
		}
	}

	private object CloneEllipsisItemSource(ObservableCollection<object> ellipsisItemsSource)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		// A copy of the hidden elements array in BreadcrumbLayout is created
		// to avoid getting a Layout cycle exception
		var newItemsSource = new ObservableCollection<object>();

		// The new list contains all the elements in reverse order
		int itemsSourceSize = ellipsisItemsSource.Count;
		if (itemsSourceSize > 0)
		{
			for (int i = itemsSourceSize - 1; i >= 0; --i)
			{
				var item = ellipsisItemsSource[i];
				newItemsSource.Add(item);
			}
		}

		return newItemsSource;
	}

	// Visual State updaters

	private void UpdateItemTypeVisualState()
	{
		VisualStateManager.GoToState(this, _isEllipsisDropDownItem ? _ellipsisDropDownStateName : _inlineStateName, false);
	}

	private void UpdateEllipsisDropDownItemVisualState(bool useTransitions)
	{
		Debug.Assert(_isEllipsisDropDownItem);

		string commonVisualStateName;

		if (!IsEnabled)
			commonVisualStateName = _disabledStateName;
		else if (_isPressed)
			commonVisualStateName = _pressedStateName;
		else if (_isPointerOver)
			commonVisualStateName = _pointerOverStateName;
		else
			commonVisualStateName = _normalStateName;

		VisualStateManager.GoToState(this, commonVisualStateName, useTransitions);
	}

	private void UpdateInlineItemTypeVisualState(bool useTransitions)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		bool isLeftToRight = (FlowDirection == FlowDirection.LeftToRight);
		string visualStateName;

		if (_isEllipsisItem)
			visualStateName = isLeftToRight ? _ellipsisStateName : _ellipsisRTLStateName;
		else if (_isLastItem)
			visualStateName = _lastItemStateName;
		else if (isLeftToRight)
			visualStateName = _defaultStateName;
		else
			visualStateName = _defaultRTLStateName;

		VisualStateManager.GoToState(this, visualStateName, useTransitions);
	}

	private void UpdateButtonVisualState(bool useTransitions)
	{
		Debug.Assert(!_isEllipsisDropDownItem);

		if (_itemButton is { } button)
		{
			string commonVisualStateName = "";

			// If is last item: place Current as prefix for visual state
			if (_isLastItem)
				commonVisualStateName = _currentStateName;

			if (!button.IsEnabled)
				commonVisualStateName = commonVisualStateName + _disabledStateName;
			else if (button.IsPressed)
				commonVisualStateName = commonVisualStateName + _pressedStateName;
			else if (button.IsPointerOver)
				commonVisualStateName = commonVisualStateName + _pointerOverStateName;
			else
				commonVisualStateName = commonVisualStateName + _normalStateName;

			VisualStateManager.GoToState(button, commonVisualStateName, useTransitions);
		}
	}
}
