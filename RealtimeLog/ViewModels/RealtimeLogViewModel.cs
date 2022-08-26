using CODE.Framework.Wpf.Mvvm;
using FileFilterX.Library;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

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

	private void AddActions()
	{
		_parent.Loaded += MainWindow_Loaded;
		AddButton = new ViewAction("Add", canExecute: (a, o) => !Loading, execute: (a, o) => AddButton_Click());
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

			Loading = false;
		}
	}

	internal void AddButton_Click()
	{
		DoFileSelect();
	}

	internal void MenuButton_Click()
	{

	}

	internal void HomeButton_Click()
	{

	}

	internal void CloseButton_Click()
	{
		_parent.Close();
	}

	internal void MainWindow_Closing()
	{

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
	#endregion
}
