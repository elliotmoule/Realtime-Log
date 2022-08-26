# Realtime-Log
Small C# .NET WPF application for watching text and log files in real time.

## Features
Some of the current features are:
  - Able to select text and log files.
  - Able to activate a selected file, with a window showing the file in it's current state.
  - A small note at the top right of each window provides the time of the last change (since the window was opened), and the change type (addition or removal).
  - App detects interruptions.
  - App code provides the ability to set a different wait timespan between reads, but yet to add GUI for changing during runtime.
  - Functionality to cancel reading of files (cancellation token provided, with ability to request cancellation).

## Future
Enhancements will include:
  1. GUI for timespan change.
  2. GUI for stopping a file read operation (request cancellation).
  3. Saving selected files, so that they're automatically provided on open.
  4. Activate windows which were open during a previous run.
  5. Better information around interruptions: change file watch status, and provide a note on the activated window that file shown isn't live.
