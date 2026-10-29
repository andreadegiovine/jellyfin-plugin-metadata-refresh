namespace Jellyfin.Plugin.MetadataRefresh.Models;

/// <summary>
/// Outcome of a refresh run.
/// </summary>
public class RefreshResult
{
    /// <summary>
    /// Gets or sets the number of libraries refreshed without errors.
    /// </summary>
    public int LibrariesRefreshed { get; set; }

    /// <summary>
    /// Gets or sets the number of libraries whose refresh failed.
    /// </summary>
    public int LibrariesFailed { get; set; }

    /// <summary>
    /// Gets or sets the number of selected libraries that no longer exist.
    /// </summary>
    public int LibrariesMissing { get; set; }
}
