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
	public sealed class StandardItem
	{
		public string Name { get; }

		public string Path { get; }

		public StandardItem(string name)
		{
			Name = name;
		}
	}

	public sealed partial class MainWindow : Window
	{
		public ObservableCollection<StandardItem> Items { get; } = [];

		public MainWindow()
		{
			Items.Add(new("Item 1"));
			Items.Add(new("Item 2"));
			Items.Add(new("Item 3"));
			Items.Add(new("Item 4"));
			Items.Add(new("Item 5"));
			Items.Add(new("Item 6"));

			this.InitializeComponent();

			ExtendsContentIntoTitleBar = true;
		}
	}
}
