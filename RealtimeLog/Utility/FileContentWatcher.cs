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
		Interval = interval ?? new ConcreteTimespan(new TimeSpan(0, 0, 2));
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

	private IEnumerable<string> ReadContent()
	{
		try
		{
			return File.ReadAllLines(Path);
		}
		catch
		{
			return null;
		}
	}

	private FileChangeType CompareContent(IEnumerable<string> newContent)
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
				var diff = newContent.Except(LastContentRead).ToList();

				changeType = diff.Count == 0 ? (newContent.Count() < LastContentRead.Count() ? FileChangeType.Removal : FileChangeType.None) : FileChangeType.Addition;
			}

			LastContentRead = newContent;
		}

		return changeType;
	}

	private void WaitOnInterval()
	{
		if (Interval != null)
			Thread.Sleep(Interval.TimeSpan);
	}

	public IEnumerable<string> LastContentRead { get; set; } = null;

	private void OnFileChanged(FileChangedEventArgs e)
	{
		FileChanged?.Invoke(this, e);
	}

	public void Dispose()
	{

	}

	public event EventHandler<FileChangedEventArgs> FileChanged;
}

internal class ConcreteTimespan
{
	public TimeSpan TimeSpan { get; private set; } = new TimeSpan(0, 0, 2);

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

public enum FileChangeType
{
	None,
	Initial,
	Removal,
	Addition,
	Interrupted
}