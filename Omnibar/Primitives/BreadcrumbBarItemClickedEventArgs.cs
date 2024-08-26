// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Omnibar;

public sealed partial class BreadcrumbBarItemClickedEventArgs
{
	public object Item { get; }

	public int Index { get; }

	public BreadcrumbBarItemClickedEventArgs(object item, int index)
	{
		Item = item;
		Index = index;
	}
}
