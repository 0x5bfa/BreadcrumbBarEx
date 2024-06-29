// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnibar
{
	public partial class BreadcrumbBarItemEx
	{
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				nameof(TextProperty),
				typeof(string),
				typeof(BreadcrumbBarItemEx),
				new PropertyMetadata(string.Empty));

		public string Path
		{
			get => (string)GetValue(PathProperty);
			set => SetValue(PathProperty, value);
		}

		public static readonly DependencyProperty PathProperty =
			DependencyProperty.Register(
				nameof(PathProperty),
				typeof(string),
				typeof(BreadcrumbBarItemEx),
				new PropertyMetadata(string.Empty));

		public bool HasChildren
		{
			get => (bool)GetValue(HasChildrenProperty);
			set => SetValue(HasChildrenProperty, value);
		}

		public static readonly DependencyProperty HasChildrenProperty =
			DependencyProperty.Register(
				nameof(HasChildrenProperty),
				typeof(bool),
				typeof(BreadcrumbBarItemEx),
				new PropertyMetadata(string.Empty));
	}
}
