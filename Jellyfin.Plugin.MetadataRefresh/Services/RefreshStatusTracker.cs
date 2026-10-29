using System;
using Jellyfin.Plugin.MetadataRefresh.Models;

namespace Jellyfin.Plugin.MetadataRefresh.Services;

/// <inheritdoc />
public sealed class RefreshStatusTracker : IRefreshStatusTracker
{
    private readonly object _lock = new();
    private RefreshStatus _status = new();

    /// <inheritdoc />
    public RefreshStatus GetSnapshot()
    {
        lock (_lock)
        {
            return Copy(_status);
        }
    }

    /// <inheritdoc />
    public void Begin(string options)
    {
        lock (_lock)
        {
            _status = new RefreshStatus
            {
                State = RefreshState.Running,
                Options = options,
                StartedAt = DateTimeOffset.Now
            };
        }
    }

    /// <inheritdoc />
    public void SetLibrariesTotal(int librariesTotal)
    {
        lock (_lock)
        {
            _status.LibrariesTotal = librariesTotal;
        }
    }

    /// <inheritdoc />
    public void SetCurrentLibrary(string name, int index)
    {
        lock (_lock)
        {
            _status.CurrentLibrary = name;
            _status.CurrentLibraryIndex = index;
        }
    }

    /// <inheritdoc />
    public void ReportProgress(double percent)
    {
        lock (_lock)
        {
            if (_status.State == RefreshState.Running && percent > _status.Progress)
            {
                _status.Progress = Math.Min(percent, 100);
            }
        }
    }

    /// <inheritdoc />
    public void Finish(RefreshState state, string message)
    {
        lock (_lock)
        {
            if (state == RefreshState.Completed)
            {
                _status.Progress = 100;
            }

            _status.State = state;
            _status.Message = message;
            _status.FinishedAt = DateTimeOffset.Now;
        }
    }

    private static RefreshStatus Copy(RefreshStatus source)
    {
        return new RefreshStatus
        {
            State = source.State,
            Progress = source.Progress,
            CurrentLibrary = source.CurrentLibrary,
            CurrentLibraryIndex = source.CurrentLibraryIndex,
            LibrariesTotal = source.LibrariesTotal,
            Options = source.Options,
            Message = source.Message,
            StartedAt = source.StartedAt,
            FinishedAt = source.FinishedAt
        };
    }
}
