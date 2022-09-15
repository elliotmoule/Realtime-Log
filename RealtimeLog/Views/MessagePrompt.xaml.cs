using RealtimeLog.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace RealtimeLog.Views;
/// <summary>
/// Interaction logic for MessagePrompt.xaml
/// </summary>
public partial class MessagePrompt : Window
{
	private MessagePromptViewModel _parent;
	public MessagePrompt(MessagePromptViewModel viewModel)
	{	
		_parent = viewModel;
		DataContext = _parent;
		InitializeComponent();
	}

	private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}
}
