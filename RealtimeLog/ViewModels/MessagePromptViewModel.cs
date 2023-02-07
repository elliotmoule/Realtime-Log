using CODE.Framework.Wpf.Mvvm;
using RealtimeLog.Views;
using System.Windows;

namespace RealtimeLog.ViewModels;
public class MessagePromptViewModel : ViewModel
{
	private readonly MessagePrompt _messageDialog;
	public MessagePromptViewModel(string message, string caption, bool showCancel = false)
	{
		AddActions();
		_messageDialog = new MessagePrompt(this);
		this.Message = message;
		this.Caption = caption;

		if (showCancel)
		{
			ShowCancel = Visibility.Visible;
		}
	}

	internal bool? ShowDialog()
	{
		var result = _messageDialog?.ShowDialog();
		DialogResult = _messageDialog?.DialogResult;

		return result;
	}

	#region Actions
	public IViewAction OK { get; set; }
	public IViewAction Cancel { get; set; }

	private void AddActions()
	{
		OK = new ViewAction("OK", execute: (a, o) => DoAccept());
		Cancel = new ViewAction("Cancel", execute: (a, o) => DoCancel());
	}

	private void DoAccept()
	{
		_messageDialog.DialogResult = true;
		_messageDialog.Close();
	}

	private void DoCancel()
	{
		_messageDialog.DialogResult = false;
		_messageDialog.Close();
	}
	#endregion

	#region Properties
	private string _message = string.Empty;
	public string Message
	{
		get { return _message; }
		set
		{
			_message = value;
			NotifyChanged(nameof(Message));
		}
	}

	private string _caption = string.Empty;
	public string Caption
	{
		get { return _caption; }
		set
		{
			_caption = value;
			NotifyChanged(nameof(Caption));
		}
	}

	public bool? DialogResult { get; private set; }

	private Visibility _showCancel = Visibility.Collapsed;
	public Visibility ShowCancel
	{
		get { return _showCancel; }
		set
		{
			_showCancel = value;
			NotifyChanged(nameof(ShowCancel));
		}
	}
	#endregion
}
