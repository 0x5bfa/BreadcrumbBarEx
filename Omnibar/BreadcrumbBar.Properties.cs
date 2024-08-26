// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Omnibar;

public partial class BreadcrumbBarEx : Control
{
	public object? ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public static DependencyProperty ItemsSourceProperty { get; } =
		DependencyProperty.Register(
			nameof(ItemsSource),
			typeof(object),
			typeof(BreadcrumbBarEx),
			new(null, (d, e) => ((BreadcrumbBarEx)d).OnPropertyChanged(e)));

	public object? ItemTemplate
	{
		get => GetValue(ItemTemplateProperty);
		set => SetValue(ItemTemplateProperty, value);
	}

	public static DependencyProperty ItemTemplateProperty { get; } =
		DependencyProperty.Register(
			nameof(ItemTemplate),
			typeof(object),
			typeof(BreadcrumbBarEx),
			new(null, (d, e) => ((BreadcrumbBarEx)d).OnPropertyChanged(e)));

	protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
	{
		if (args.Property == ItemsSourceProperty)
		{
			UpdateItemsRepeaterItemsSource();
		}
		else if (args.Property == ItemTemplateProperty)
		{
			UpdateItemTemplate();
			UpdateEllipsisItemTemplate();
		}
	}

	private void UpdateItemsRepeaterItemsSource()
	{
		_breadcrumbItemsSourceView = null;

		if (ItemsSource != null)
		{
			_breadcrumbItemsSourceView = new(ItemsSource);

			if (_itemsRepeater is { } itemsRepeater)
			{
				_itemsEnumerable = new(ItemsSource);
				itemsRepeater.ItemsSource = _itemsEnumerable;
			}

			if (_breadcrumbItemsSourceView != null)
			{
				_breadcrumbItemsSourceView.CollectionChanged -= OnBreadcrumbBarItemsSourceCollectionChanged;
				_breadcrumbItemsSourceView.CollectionChanged += OnBreadcrumbBarItemsSourceCollectionChanged;
			}
		}
	}

	private void UpdateItemTemplate()
	{
		if (ItemTemplate is not null &&
			_itemsRepeater is not null)
			_itemsRepeater.ItemTemplate = ItemTemplate;
	}

	private void UpdateEllipsisItemTemplate()
	{
		var newItemTemplate = ItemTemplate;

		// Copy the item template to the ellipsis item too
		if (_ellipsisBreadcrumbBarItem is { } ellipsisBreadcrumbBarItem)
			ellipsisBreadcrumbBarItem.SetDataTemplate(newItemTemplate);
	}

	public event TypedEventHandler<BreadcrumbBarEx, BreadcrumbBarItemClickedEventArgs>? ItemClicked;
}
