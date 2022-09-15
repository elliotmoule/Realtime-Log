using CODE.Framework.Wpf.Mvvm;
using RealtimeLog.Views;

namespace RealtimeLog.ViewModels;
public class MessagePromptViewModel : ViewModel
{
	private readonly MessagePrompt _messageDialog;
	public MessagePromptViewModel(string message, string caption)
	{
		AddActions();
		_messageDialog = new MessagePrompt(this);
		this.Message = message;
		this.Caption = caption;
	}

	internal bool? ShowDialog()
	{
		var result = _messageDialog?.ShowDialog();
		DialogResult = _messageDialog?.DialogResult;

		return result;
	}

	#region Actions
	public IViewAction OK { get; set; }

	private void AddActions()
	{
		OK = new ViewAction("OK", execute: (a, o) => DoAccept());
	}

	private void DoAccept()
	{
		_messageDialog.DialogResult = true;
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
	#endregion
}
