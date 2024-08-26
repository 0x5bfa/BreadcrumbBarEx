// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using System.Collections;
using System.Collections.Generic;

namespace Omnibar;

internal partial class BreadcrumbBarEnumerable : IEnumerable<object?>
{
	public object? ItemsSource { get; }

	public BreadcrumbBarEnumerable()
	{
	}

	public BreadcrumbBarEnumerable(object? itemsSource)
	{
		ItemsSource = itemsSource;
	}

	public IEnumerator<object?> GetEnumerator()
	{
		return new BreadcrumbBarEnumerator(ItemsSource);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
