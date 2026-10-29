using Jellyfin.Plugin.MetadataRefresh.Models;

namespace Jellyfin.Plugin.MetadataRefresh.Services;

/// <summary>
/// Keeps the progress of the refresh in memory, so the settings page of the plugin can show it
/// while the scheduled task is running.
/// </summary>
public interface IRefreshStatusTracker
{
    /// <summary>
    /// Gets a copy of the current status.
    /// </summary>
    /// <returns>The status.</returns>
    RefreshStatus GetSnapshot();

    /// <summary>
    /// Starts tracking a new run.
    /// </summary>
    /// <param name="options">Short description of the options of the run.</param>
    void Begin(string options);

    /// <summary>
    /// Sets the number of libraries of the run.
    /// </summary>
    /// <param name="librariesTotal">The number of libraries to refresh.</param>
    void SetLibrariesTotal(int librariesTotal);

    /// <summary>
    /// Sets the library that is being refreshed.
    /// </summary>
    /// <param name="name">The name of the library.</param>
    /// <param name="index">The position of the library, starting from 1.</param>
    void SetCurrentLibrary(string name, int index);

    /// <summary>
    /// Reports the progress of the run. The value never decreases during a run.
    /// </summary>
    /// <param name="percent">The progress, from 0 to 100.</param>
    void ReportProgress(double percent);

    /// <summary>
    /// Ends the run.
    /// </summary>
    /// <param name="state">The final state.</param>
    /// <param name="message">A message about the outcome.</param>
    void Finish(RefreshState state, string message);
}
