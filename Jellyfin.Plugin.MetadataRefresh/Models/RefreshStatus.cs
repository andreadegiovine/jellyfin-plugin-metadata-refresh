using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MetadataRefresh.Models;

/// <summary>
/// Snapshot of the progress of the refresh, returned by the status endpoint of the plugin.
/// </summary>
public class RefreshStatus
{
    /// <summary>
    /// Gets or sets the state of the refresh.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RefreshState State { get; set; }

    /// <summary>
    /// Gets or sets the progress of the running (or of the last) refresh, from 0 to 100.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Gets or sets the name of the library that is being refreshed.
    /// </summary>
    public string CurrentLibrary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position of the library that is being refreshed (starting from 1).
    /// </summary>
    public int CurrentLibraryIndex { get; set; }

    /// <summary>
    /// Gets or sets the number of libraries to refresh.
    /// </summary>
    public int LibrariesTotal { get; set; }

    /// <summary>
    /// Gets or sets a short description of the options of the run.
    /// </summary>
    public string Options { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a message about the state, for example the reason why the run was skipped.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the moment the last run started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the moment the last run finished.
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }
}
