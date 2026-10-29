namespace Jellyfin.Plugin.MetadataRefresh.Models;

/// <summary>
/// State of the refresh, as shown in the settings page of the plugin.
/// </summary>
public enum RefreshState
{
    /// <summary>
    /// The task did not run since the server was started.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// The task is running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// The last run completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The last run was cancelled.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// The last run failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The last run did nothing, for example because no library is selected.
    /// </summary>
    Skipped = 5
}
