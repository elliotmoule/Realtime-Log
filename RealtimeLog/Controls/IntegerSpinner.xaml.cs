using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RealtimeLog.Controls;
/// <summary>
/// Interaction logic for IntegerSpinner.xaml
/// </summary>
public partial class IntegerSpinner : UserControl
{
    public IntegerSpinner()
    {
        InitializeComponent();
    }

    public int IntValue
    {
        get { return (int)GetValue(IntValueProperty); }
        set
        {
            SetValue(IntValueProperty, value);
        }
    }

    public static readonly DependencyProperty IntValueProperty =
        DependencyProperty.Register("IntValue", typeof(int), typeof(IntegerSpinner),
            new FrameworkPropertyMetadata(
                defaultValue: 0,
                flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                propertyChangedCallback: OnIntValueChanged));

    public int? Min
    {
        get { return (int?)GetValue(MinProperty); }
        set { SetValue(MinProperty, value); }
    }

    public static readonly DependencyProperty MinProperty =
        DependencyProperty.Register("Min", typeof(int?), typeof(IntegerSpinner), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public int? Max
    {
        get { return (int?)GetValue(MaxProperty); }
        set { SetValue(MaxProperty, value); }
    }

    public static readonly DependencyProperty MaxProperty =
        DependencyProperty.Register("Max", typeof(int?), typeof(IntegerSpinner), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    private static void OnIntValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var iS = (IntegerSpinner)sender;

        var value = iS.IntValue;
        if (iS.Min != null && value < iS.Min)
        {
            value = iS.Min.Value;
        }

        if (iS.Max != null && value > iS.Max)
        {
            value = iS.Max.Value;
        }

        if (value != iS.IntValue)
        {
            iS.IntValue = value;
        }
    }

    private void BtnIncrement_Click(object sender, RoutedEventArgs e) => IntValue++;

    private void BtnDecrement_Click(object sender, RoutedEventArgs e) => IntValue--;

    private void IntegerInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_allowedKeys.Contains(e.Key))
        {
            base.OnKeyUp(e);
        }
        else if (e.Key == Key.Up)
        {
            IntValue++;
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            IntValue--;
            e.Handled = true;
        }
        else if (e.Key == Key.Return || e.Key == Key.Enter)
        {
            IntegerInput.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
        else
        {
            e.Handled = true;
        }
    }

    private readonly List<Key> _allowedKeys =
    [
        Key.NumPad0,
        Key.NumPad1,
        Key.NumPad2,
        Key.NumPad3,
        Key.NumPad4,
        Key.NumPad5,
        Key.NumPad6,
        Key.NumPad7,
        Key.NumPad8,
        Key.NumPad9,
        Key.D0,
        Key.D1,
        Key.D2,
        Key.D3,
        Key.D4,
        Key.D5,
        Key.D6,
        Key.D7,
        Key.D8,
        Key.D9,
        Key.OemMinus,
        Key.OemPlus,
        Key.Subtract,
        Key.Add,
        Key.Tab,
        Key.Left,
        Key.Right,
        Key.Delete,
        Key.Back
    ];
}
