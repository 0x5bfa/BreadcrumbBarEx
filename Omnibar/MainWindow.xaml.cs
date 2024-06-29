using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Omnibar
{
	public sealed partial class MainWindow : Window
	{
		public ObservableCollection<string> Items { get; } = [];

		public MainWindow()
		{
			this.InitializeComponent();

			ExtendsContentIntoTitleBar = true;

			Items.Add("Item 1");
			Items.Add("Item 2");
			Items.Add("Item 3");
			Items.Add("Item 4");
			Items.Add("Item 5");
			Items.Add("Item 6");
		}
	}
}
