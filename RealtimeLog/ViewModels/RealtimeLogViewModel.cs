using CODE.Framework.Wpf.Controls;
using CODE.Framework.Wpf.Mvvm;
using FileFilterX.Library;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RealtimeLog;
public class RealtimeLogViewModel : ViewModel
{
	MainWindow _parent;
	public RealtimeLogViewModel(MainWindow parent)
	{
		_parent = parent;
		AddActions();
	}

	#region Actions
	public IViewAction AddButton { get; set; }
	public IViewAction Quit { get; set; }
	public IViewAction Help { get; set; }
	public IViewAction ClearList { get; set; }
	public IViewAction CloseAll { get; set; }
	public IViewAction LoadList { get; set; }
	public IViewAction SaveList { get; set; }
	public IViewAction AutoLoadSelect { get; set; }

	private void AddActions()
	{
		_parent.Loaded += MainWindow_Loaded;
		AddButton = new ViewAction("Add", canExecute: (a, o) => !Loading, execute: (a, o) => AddButton_Click());

		Quit = new ViewAction("Quit", canExecute: (a, o) => MenuToggle && IsFormEnabled, execute: (a, o) => CloseButton_Click());
		Help = new ViewAction("Help", canExecute: (a, o) => MenuToggle && IsFormEnabled, execute: (a, o) => DoHelp());
		ClearList = new ViewAction("Clear", canExecute: (a, o) => MenuToggle && IsFormEnabled && AllWatchFiles.Count > 0, execute: (a, o) => DoClearList());
		CloseAll = new ViewAction("CloseAll", canExecute: (a, o) => MenuToggle && IsFormEnabled && AllWatchFiles.Any(x => x.Active), execute: (a, o) => DoCloseAll());
		LoadList = new ViewAction("Load", canExecute: (a, o) => MenuToggle && IsFormEnabled, execute: (a, o) => DoLoadList());
		SaveList = new ViewAction("Save", canExecute: (a, o) => MenuToggle && IsFormEnabled, execute: (a, o) => DoSaveList());
		AutoLoadSelect = new ViewAction("AutoLoad", canExecute: (a, o) => MenuToggle && IsFormEnabled, execute: (a, o) => DoAutoLoadSelect());
	}

	internal void Invalidate()
	{
		InvalidateAllActions();

		Quit?.InvalidateCanExecute();
		Help?.InvalidateCanExecute();
		ClearList?.InvalidateCanExecute();
		CloseAll?.InvalidateCanExecute();
		LoadList?.InvalidateCanExecute();
		SaveList?.InvalidateCanExecute();
		AutoLoadSelect?.InvalidateCanExecute();
	}

	private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
	{
		Loading = false;
	}

	private void DoFileSelect()
	{
		if (!Loading)
		{
			Loading = true;
			var select = new OpenFileDialog
			{
				CheckFileExists = true,
				Multiselect = true,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
				RestoreDirectory = true,
				Title = "Select all files to watch",
				Filter = FileFilterX.Library.FileFilterX.FilterBuilder(false,
																		new Filter("Log Files", "txt", "TXT", "log", "LOG"))
			};

			select.ShowDialog();

			var fileNames = new List<string>();
			if (select.FileNames is string[] filePaths)
			{
				fileNames.AddRange(filePaths);
			}

			if (select.FileName is string filePath
				&& !fileNames.Contains(filePath))
			{
				fileNames.Add(filePath);
			}

			foreach (var fileName in fileNames)
			{
				if (!AllWatchFiles.Any(x => x.Path.Equals(fileName)))
				{
					AllWatchFiles.Add(new ActiveWindowViewModel(this, fileName));
				}
			}

			InvalidateAllActions();
			Loading = false;
		}
	}

	internal void AddButton_Click()
	{
		DoFileSelect();
	}

	internal void MenuButton_Click()
	{
		MenuToggle = !MenuToggle;
	}

	internal void AnyWhere_Click(MouseButtonEventArgs e)
	{
		if (MenuToggle
			&& !_parent.MenuOverlay.IsMouseOver)
		{
			MenuToggle = false;
		}
	}

	internal void CloseButton_Click()
	{
		_parent.Close();
	}

	internal void MainWindow_Closing()
	{

	}

	private void DoHelp()
	{

	}

	private void DoClearList()
	{
		AllWatchFiles.Clear();
		NotifyChanged(nameof(AllWatchFiles));
		InvalidateAllActions();
	}

	private void DoCloseAll()
	{
		if (AllWatchFiles != null && AllWatchFiles.Count > 0)
		{
			for (int i = 0; i < AllWatchFiles.Count; i++)
			{
				AllWatchFiles[i].ActivateWindow();
			}
		}
	}

	private void DoLoadList()
	{
		var file = Load();
		if (!string.IsNullOrWhiteSpace(file)
				&& File.Exists(file))
		{
			IsFormEnabled = false;

			string json = File.ReadAllText(file);

			var loadedFiles = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);

			foreach (var item in loadedFiles)
			{
				AllWatchFiles.Add(new ActiveWindowViewModel(this, item.Key, item.Value));
			}

			IsFormEnabled = true;
		}
	}

	private void DoSaveList()
	{
		var saveFileDialog = new SaveFileDialog
		{
			CreatePrompt = false,
			DefaultExt = "rtls",
			Filter = FileFilterX.Library.FileFilterX.FilterBuilder(true, new Filter("Realtime Log Save File", "rtls")),
			RestoreDirectory = true,
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
			OverwritePrompt = true,
			ValidateNames = true
		};

		saveFileDialog.ShowDialog();

		if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
		{
			IsFormEnabled = false;

			var paths = new Dictionary<string, bool>();
			for (int i = 0; i < AllWatchFiles.Count; i++)
			{
				paths.Add(AllWatchFiles[i].Path, AllWatchFiles[i].Active);
			}

			string json = JsonSerializer.Serialize(paths);

			File.WriteAllText(Path.ChangeExtension(saveFileDialog.FileName, "rtls"), json);

			IsFormEnabled = true;
		}
	}

	private void DoAutoLoadSelect()
	{

	}

	private string Load()
	{
		IsFormEnabled = false;

		var openFileDialog = new OpenFileDialog
		{
			DefaultExt = "rtls",
			Filter = FileFilterX.Library.FileFilterX.FilterBuilder(true, new Filter("Realtime Log Save File", "rtls")),
			RestoreDirectory = true,
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
			ValidateNames = true,
			Multiselect = false
		};

		openFileDialog.ShowDialog();

		IsFormEnabled = true;

		return openFileDialog.FileName;
	}
	#endregion

	#region Functional
	internal void UpdateActiveCount()
	{
		NotifyChanged(nameof(ActiveWindows));
	}
	#endregion

	#region Properties
	public int ActiveWindows
	{
		get { return AllWatchFiles.Count(x => x.Active); }
	}

	private ObservableCollection<ActiveWindowViewModel> _allWatchFiles = new();
	public ObservableCollection<ActiveWindowViewModel> AllWatchFiles
	{
		get { return _allWatchFiles; }
		set
		{
			_allWatchFiles = value;
			NotifyChanged(nameof(AllWatchFiles));
		}
	}

	private bool _loading = false;
	public bool Loading
	{
		get { return _loading; }
		set
		{
			_loading = value;
			NotifyChanged(nameof(Loading));

			LoadingStatusText = value ? "Loading..." : "Ready";
			LoadingStatusFill = value ? ProjectConstants.LoadingColor : ProjectConstants.NotLoadingColor;

			InvalidateAllActions();
		}
	}

	private string _loadingStatusText = "Loading...";
	public string LoadingStatusText
	{
		get { return _loadingStatusText; }
		private set
		{
			_loadingStatusText = value;
			NotifyChanged(nameof(LoadingStatusText));
		}
	}

	private Brush _loadingStatusFill = ProjectConstants.UnknownBrush;
	public Brush LoadingStatusFill
	{
		get { return _loadingStatusFill; }
		private set
		{
			_loadingStatusFill = value;
			NotifyChanged(nameof(LoadingStatusFill));
		}
	}

	private bool _menuToggle = false;
	public bool MenuToggle
	{
		get { return _menuToggle; }
		set
		{
			_menuToggle = value;
			NotifyChanged(nameof(MenuToggle));

			if (_parent.MenuOverlay.Width == 0 || _parent.MenuOverlay.Width == 200)
			{
				var widthAnimation = new DoubleAnimation(value ? 200 : 0, new Duration(TimeSpan.FromSeconds(0.3)));
				_parent.MenuOverlay.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
			}

			Invalidate();
		}
	}

	private bool _isFormEnabled = true;
	public bool IsFormEnabled
	{
		get { return _isFormEnabled; }
		set
		{
			_isFormEnabled = value;
			NotifyChanged(nameof(IsFormEnabled));
			Invalidate();
		}
	}
	#endregion
}
