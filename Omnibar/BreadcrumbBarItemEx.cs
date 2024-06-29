// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Omnibar
{
	public partial class BreadcrumbBarItemEx : BreadcrumbBarItem
	{
		public BreadcrumbBarItemEx()
		{
			DefaultStyleKey = typeof(BreadcrumbBarItemEx);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_ChevronButton") is Button button)
			{
				button.RegisterPropertyChangedCallback(ButtonBase.IsPressedProperty, (sender, args) =>
				{
					VisualStateManager.GoToState(button, "Pressed", true);
					VisualStateManager.GoToState(button, "Expanded", true);
				});

				button.RegisterPropertyChangedCallback(ButtonBase.IsPointerOverProperty, (sender, args) =>
				{
					VisualStateManager.GoToState(button, "PointerOver", true);
				});
			}
		}
	}
}
