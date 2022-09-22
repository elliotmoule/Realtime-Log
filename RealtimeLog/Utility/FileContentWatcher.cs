using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeLog.Utility;
internal class FileContentWatcher : IDisposable
{
	public string Path { get; private set; }
	public ConcreteTimespan Interval { get; set; }
	private CancellationTokenSource _token;

	public FileContentWatcher(string path, ConcreteTimespan interval = null)
	{
		Path = path;
		Interval = interval ?? new ConcreteTimespan(new TimeSpan(0, 0, ConcreteTimespan.MinValue));
	}

	public bool StartFileWatch()
	{
		return FileWatch();
	}

	private bool FileWatch()
	{
		_token = new CancellationTokenSource();
		var fileInfo = new FileInfo(Path);

		if (fileInfo.Exists)
		{
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if (_token.IsCancellationRequested)
					{
						return;
					}

					OnFileChecked(new FileCheckedEventArgs { CheckTime = DateTime.Now });

					if (fileInfo.Exists)
					{
						if (CompareContent(ReadContent()) is FileChangeType f
								&& f != FileChangeType.None)
						{
							var args = new FileChangedEventArgs
							{
								FileChangeType = f,
								ChangeTime = DateTime.Now
							};

							OnFileChanged(args);
						}
					}

					WaitOnInterval();
				}
			}, _token.Token);

			return true;
		}

		return false;
	}

	public bool StopFileWatch()
	{
		try
		{
			_token.Cancel();

			return true;
		}
		catch
		{
			return false;
		}
	}

	private List<string> ReadContent()
	{
		try
		{
			var readContents = new List<string>();
			using (var reader = new StreamReader(Path, System.Text.Encoding.UTF8))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					readContents.Add(line);
				}
			}
			return readContents;
		}
		catch
		{
			return null;
		}
	}

	private FileChangeType CompareContent(List<string> newContent)
	{
		var changeType = FileChangeType.None;

		if (newContent == null)
		{
			changeType = FileChangeType.Interrupted;
		}
		else
		{
			if (LastContentRead != null)
			{
				var newRead = newContent.Count;
				var oldRead = LastContentRead.Count;

				if (newRead > oldRead)
				{
					changeType = FileChangeType.Addition;
				}
				else if(newRead < oldRead)
				{
					changeType = FileChangeType.Removal;
				}
				else
				{
					// Same number of lines. Do Diff
					var diff = newContent.Except(LastContentRead);
					changeType = diff.Count() > 0 ? FileChangeType.Update : FileChangeType.None;
				}
			}

			LastContentRead = newContent;
		}

		return changeType;
	}

	private void WaitOnInterval()
	{
		Interval ??= new ConcreteTimespan(new TimeSpan(0, 0, ConcreteTimespan.MinValue));

		Thread.Sleep(Interval.TimeSpan);
	}

	public List<string> LastContentRead { get; set; } = null;

	private void OnFileChanged(FileChangedEventArgs e)
	{
		FileChanged?.Invoke(this, e);
	}

	private void OnFileChecked(FileCheckedEventArgs e)
	{
		FileChecked?.Invoke(this, e);
	}

	public void Dispose()
	{

	}

	public event EventHandler<FileChangedEventArgs> FileChanged;
	public event EventHandler<FileCheckedEventArgs> FileChecked;
}

internal class ConcreteTimespan
{
	public TimeSpan TimeSpan { get; private set; } = new TimeSpan(0, 0, MinValue);
	public static int MinValue { get; } = 2;

	public ConcreteTimespan(TimeSpan timeSpan)
	{
		if (timeSpan > new TimeSpan(0, 0, 2))
			TimeSpan = timeSpan;
	}
}

internal class FileChangedEventArgs : EventArgs
{
	public FileChangeType FileChangeType { get; set; }
	public DateTime ChangeTime { get; set; }
}

internal class FileCheckedEventArgs : EventArgs
{
	public DateTime CheckTime { get; set; }
}

public enum FileChangeType
{
	None,
	Initial,
	Removal,
	Addition,
	Update,
	Interrupted
}