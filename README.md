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
  - GUI for changing the interval between file reads.
  - GUI for stopping/starting log reads (for whether it's real time).
  - Button to clear the current log.
  - Auto-scroll to the bottom on log update.

## Future
Enhancements will include:
  1. Add side menu for adding extra options to the main window (quit, start up active windows, load saved list, clear all, close all).
  1. Saving selected files, so that they're automatically provided on open.
  2. Activate windows which were open during a previous run.
  3. Better information around interruptions: change file watch status, and provide a note on the activated window that file shown isn't live.
