using CODE.Framework.Wpf.Mvvm;
using RealtimeLog.Utility;
using System;
using System.IO;
using System.Threading;
using System.Windows.Media;

namespace RealtimeLog;
public class ActiveWindowViewModel : ViewModel
{
	RealtimeLogViewModel _parent;
	ActiveWindow _window;
	FileContentWatcher _fileWatcher;

	public ActiveWindowViewModel(RealtimeLogViewModel parent, string path, bool active = false)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentNullException(nameof(path));
		}
		_parent = parent;
		AddActions();
		Id = Guid.NewGuid();
		Path = path;

		if (active)
		{
			Active = true;
			ActivateWindow();
		}
	}

	#region Actions
	private void AddActions()
	{
		Activate = new ViewAction(id: "Activate", caption: "Activate Window",
			canExecute: (a, o) => Exists && FileWatchStatus == FileWatchStatus.Good,
			execute: (a, o) => DoActivate());

		Refresh = new ViewAction("Refresh", execute: (a, o) => DoRefresh());
		Remove = new ViewAction("Remove", execute: (a, o) => DoRemove());
	}

	public IViewAction Activate { get; set; }
	public IViewAction Refresh { get; set; }
	public IViewAction Remove { get; set; }

	internal void DoActivate()
	{
		UpdatePathStatus();
		ToggleActive();
		ActivateWindow();
	}

	private void DoRefresh()
	{
		UpdatePathStatus();
		ActivateWindow();
	}

	private void DoRemove()
	{
		_parent.AllWatchFiles.Remove(this);
	}
	#endregion

	private void WatchFile()
	{
		_fileWatcher = new FileContentWatcher(Path, new ConcreteTimespan(new TimeSpan(0, 0, WatchInterval)));

		_fileWatcher.FileChanged += FileWatcher_Changed;

		_fileWatcher.FileChecked += FileWatcher_FileChecked;

		_fileWatcher.StartFileWatch();

		FileRead(FileChangeType.Initial);
	}

	private void FileWatcher_FileChecked(object sender, FileCheckedEventArgs e)
	{
		DateTimeLastChecked = e.CheckTime;
	}

	private void FileWatcher_Changed(object sender, FileChangedEventArgs e)
	{
		FileRead(e.FileChangeType);
	}

	private void FileRead(FileChangeType changeTypes)
	{
		if (Active && Exists)
		{
			if (changeTypes == FileChangeType.Addition || changeTypes == FileChangeType.Removal
				|| changeTypes == FileChangeType.Initial)
			{
				var text = File.ReadAllText(Path);
				OpenFileText = text;
			}

			DateTimeChanged = DateTime.Now;
			ChangeType = changeTypes;
		}
	}

	private void ToggleActive()
	{
		var toSet = !Active;

		if (!Exists ||
			FileWatchStatus != FileWatchStatus.Good)
		{
			toSet = false;
		}

		Active = toSet;
	}

	private void UpdatePathStatus(string value = "")
	{
		if (value != null)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				value = Path;
			}

			if (!string.IsNullOrWhiteSpace(value))
			{
				_fileInfo = new FileInfo(value);
				if (_fileInfo.Exists)
				{
					Exists = true;
					FileWatchStatus = FileWatchStatus.Good;
				}
				else
				{
					Exists = false;
					FileWatchStatus = FileWatchStatus.Error;
				}
				return;
			}
		}

		Exists = false;
		FileWatchStatus = FileWatchStatus.None;
	}

	internal void ActivateWindow()
	{
		// Check if Active state is true, and if so activate the window.
		// If it's false, do not activate.
		// if Active == true, and it's already open, bring it to the front.
		// if Active == false, and it's already open, close it.

		if (Active)
		{
			if (_window != null)
			{
				if (!_window.HasClosed)
				{
					_window.Activate();
				}
				else
				{
					SafelyCloseWindow();
				}
			}
			else
			{
				_window = new ActiveWindow(this);
				_window.Show();
				WatchFile();
			}
		}
		else
		{
			SafelyCloseWindow();
		}

		_parent?.UpdateActiveCount();
	}

	internal void ClearLog()
	{
		if (Exists)
		{
			File.WriteAllText(Path, string.Empty);
			FileRead(FileChangeType.Removal);
		}
	}

	internal void OpenLog()
	{
		if (Exists)
		{
			System.Diagnostics.Process.Start("notepad.exe", Path);
		}
	}

	private void SafelyCloseWindow()
	{
		if (_window != null)
		{
			CloseFileWatcher();

			if (!_window.HasClosed)
			{
				_window.Close();
			}

			AsyncWorker.Execute(() =>
			{
				var response = true;

				var count = 100000;
				while (_window != null && !_window.HasClosed
					&& count > 0)
				{
					count--;
				}

				if (count == 0)
				{
					response = false;
				}

				return response;
			}, response =>
			{
				if (response)
				{
					// Window has closed, and been disposed.
					Active = false;
					_window = null;
				}
			});
		}
	}

	private void CloseFileWatcher()
	{
		if (_fileWatcher != null)
		{
			_fileWatcher.StopFileWatch();
			_fileWatcher.FileChanged -= FileWatcher_Changed;
			_fileWatcher.FileChecked -= FileWatcher_FileChecked;
			_fileWatcher.Dispose();
			_fileWatcher = null;
		}
	}

	#region Properties
	public Guid Id { get; set; }

	private string _path;
	public string Path
	{
		get { return _path; }
		set
		{
			_path = value;

			UpdatePathStatus(value);

			NotifyChanged(nameof(Path));
			NotifyChanged(nameof(WatchFileName));
		}
	}

	private string _openFileText = "Loading...";
	public string OpenFileText
	{
		get { return _openFileText; }
		set
		{
			_openFileText = value;
			NotifyChanged(nameof(OpenFileText));
		}
	}

	private FileInfo _fileInfo;

	private bool _active;
	public bool Active
	{
		get { return _active; }
		set
		{
			_active = value;
			NotifyChanged(nameof(Active));
			_parent?.Invalidate();
		}
	}

	private bool _exists;
	public bool Exists
	{
		get { return _exists; }
		set
		{
			_exists = value;
			NotifyChanged(nameof(Exists));
		}
	}

	private FileWatchStatus _fileWatchStatus;
	public FileWatchStatus FileWatchStatus
	{
		get { return _fileWatchStatus; }
		set
		{
			_fileWatchStatus = value;
			NotifyChanged(nameof(FileWatchStatus));
		}
	}

	public string WatchFileName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Path)
				&& Exists)
			{
				return $"Watching {System.IO.Path.GetFileName(Path)}";
			}

			return string.Empty;
		}
	}

	private DateTime _dateTimeChanged = default;
	public DateTime DateTimeChanged
	{
		get { return _dateTimeChanged; }
		set
		{
			_dateTimeChanged = value;
			NotifyChanged(nameof(DateTimeChanged));
		}
	}

	private DateTime _dateTimeLastChecked = default;
	public DateTime DateTimeLastChecked
	{
		get { return _dateTimeLastChecked; }
		set
		{
			if (value != _dateTimeLastChecked)
			{
				LastCheckedBrush = Brushes.Green;
			}
			_dateTimeLastChecked = value;
			NotifyChanged(nameof(DateTimeLastChecked));

			if (LastCheckedBrush == Brushes.Green)
			{
				AsyncWorker.Execute(() =>
				{
					Thread.Sleep(1000);
					return true;
				}, response =>
				{
					LastCheckedBrush = Brushes.Black;
				});
			}
		}
	}

	private FileChangeType _changeType = FileChangeType.None;
	public FileChangeType ChangeType
	{
		get { return _changeType; }
		set
		{
			_changeType = value;
			NotifyChanged(nameof(ChangeType));
		}
	}

	private bool _realTime = true;
	public bool RealTime
	{
		get { return _realTime; }
		set
		{
			_realTime = value;

			if (!value)
			{
				CloseFileWatcher();
			}
			else
			{
				if (_fileWatcher == null)
				{
					WatchFile();
				}
			}
			NotifyChanged(nameof(RealTime));
		}
	}

	private int _oldValue = ConcreteTimespan.MinValue;
	private int _watchInterval = ConcreteTimespan.MinValue;
	public int WatchInterval
	{
		get { return _watchInterval; }
		set
		{
			_watchInterval = value;
			NotifyChanged(nameof(WatchInterval));

			if (_fileWatcher != null && _oldValue != value && value >= ConcreteTimespan.MinValue)
			{
				_fileWatcher.Interval = new ConcreteTimespan(new TimeSpan(0, 0, value));
			}
			_oldValue = value;
		}
	}

	private Brush _lastCheckedBrush = Brushes.Black;
	public Brush LastCheckedBrush
	{
		get { return _lastCheckedBrush; }
		set
		{
			_lastCheckedBrush = value;
			NotifyChanged(nameof(LastCheckedBrush));
		}
	}
	#endregion
}
