using System.Windows;

namespace RealtimeLog;

/// <summary>
/// Interaction logic for ActiveWindow.xaml
/// </summary>
public partial class ActiveWindow : Window
{
	ActiveWindowViewModel _parent;
	public ActiveWindow(ActiveWindowViewModel parent)
	{
		_parent = parent;
		DataContext = _parent;
		InitializeComponent();
		Closed += ActiveWindow_Closed;
	}

	private void ActiveWindow_Closed(object sender, System.EventArgs e)
	{
		HasClosed = true;
		_parent?.ActivateWindow();
	}

	public bool HasClosed { get; private set; }

	private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
	{
		if (sender is System.Windows.Controls.TextBox t)
		{
			t.ScrollToEnd();
		}
	}

	private void Clear_Click(object sender, RoutedEventArgs e)
	{
		_parent.ClearLog();
	}
}
