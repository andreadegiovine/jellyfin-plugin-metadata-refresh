using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MetadataRefresh.Configuration;
using Jellyfin.Plugin.MetadataRefresh.Models;

namespace Jellyfin.Plugin.MetadataRefresh.Services;

/// <summary>
/// Refreshes the metadata of the libraries selected in the plugin configuration.
/// </summary>
public interface IMetadataRefresher
{
    /// <summary>
    /// Refreshes the selected libraries one after the other, reporting the progress.
    /// </summary>
    /// <param name="configuration">The plugin configuration.</param>
    /// <param name="progress">Receives the progress, from 0 to 100.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the run.</returns>
    Task<RefreshResult> RefreshAsync(
        PluginConfiguration configuration,
        IProgress<double> progress,
        CancellationToken cancellationToken);
}
