// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace Omnibar;

public partial class BreadcrumbBarItemAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
{
	public BreadcrumbBarItemAutomationPeer(BreadcrumbBarExItem owner) : base(owner)
	{
	}

	protected override string GetLocalizedControlTypeCore()
	{
		return "BreadcrumbBarItem";
	}

	protected override object GetPatternCore(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Invoke)
			return this;

		return base.GetPatternCore(patternInterface);
	}

	protected override string GetClassNameCore()
	{
		return nameof(BreadcrumbBarExItem);
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	public void Invoke()
	{
		if (Owner is BreadcrumbBarExItem breadcrumbItem)
			breadcrumbItem.OnClick(null, null);
	}
}
