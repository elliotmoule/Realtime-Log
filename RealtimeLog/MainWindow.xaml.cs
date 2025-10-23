using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace RealtimeLog;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    readonly RealtimeLogViewModel viewModel;
    public MainWindow()
    {
        InitializeComponent();
        PreviewMouseDown += MainWindow_PreviewMouseDown;
        viewModel = new RealtimeLogViewModel(this);
        DataContext = viewModel;
    }

    private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        viewModel?.AnyWhere_Click();
    }

    private void MenuClick(object sender, RoutedEventArgs e)
    {
        viewModel?.MenuButton_Click();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Close all active windows.
        viewModel?.MainWindow_Closing();
    }

    // Window Position Reset
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
           && key == Key.Escape)
        {
            var x = (SystemParameters.PrimaryScreenWidth / 2) - (Width / 2);
            var y = (SystemParameters.PrimaryScreenHeight / 2) - (Height / 2);
            var loc = new System.Drawing.Point((int)Math.Round(x), (int)Math.Round(y));

            Application.Current.MainWindow.Left = loc.X;
            Application.Current.MainWindow.Top = loc.Y;
        }
    }

    private void MinimiseButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        viewModel?.CloseButton_Click();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        viewModel?.AddButton_Click();
    }
}
