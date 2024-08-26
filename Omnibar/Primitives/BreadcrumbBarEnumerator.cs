// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Omnibar;

internal partial class BreadcrumbBarEnumerator : IEnumerator<object?>
{
	private int _currentIndex;
	private ItemsSourceView? _breadcrumbItemsSourceView;
	private int _size;

	object? IEnumerator.Current
		=> Current;

	public object? Current
	{
		get
		{
			if (_currentIndex == 0)
				return null;
			else if (HasCurrent())
				return _breadcrumbItemsSourceView!.GetAt(_currentIndex - 1);
			else
				throw new InvalidOperationException("Out of bounds");
		}
	}

	internal BreadcrumbBarEnumerator(object? itemsSource)
	{
		_currentIndex = 0;

		if (itemsSource != null)
		{
			_breadcrumbItemsSourceView = new(itemsSource);

			// Add 1 to account for the leading null/ellipsis element
			_size = _breadcrumbItemsSourceView.Count + 1;
		}
		else
		{
			_size = 1;
		}
	}

	private bool HasCurrent()
	{
		return _currentIndex < _size;
	}

	public bool MoveNext()
	{
		if (HasCurrent())
		{
			++_currentIndex;
			return HasCurrent();
		}
		else
		{
			throw new InvalidOperationException("Out of bounds");
		}
	}

	bool IEnumerator.MoveNext()
	{
		return MoveNext();
	}

	public void Reset()
	{
	}

	public void Dispose()
	{
	}
}
