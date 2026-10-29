using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MetadataRefresh.Configuration;
using Jellyfin.Plugin.MetadataRefresh.Models;
using Jellyfin.Plugin.MetadataRefresh.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MetadataRefresh.ScheduledTasks;

/// <summary>
/// Scheduled task that refreshes the metadata of the libraries selected in the plugin settings.
/// It has no trigger by default: the schedule is added from the Scheduled Tasks page, or the task
/// is started by hand.
/// </summary>
public sealed class MetadataRefreshTask : IScheduledTask
{
    private readonly IMetadataRefresher _refresher;
    private readonly IRefreshStatusTracker _status;
    private readonly ILibraryManager _libraryManager;
    private readonly ILocalizationManager _localization;
    private readonly ILogger<MetadataRefreshTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRefreshTask"/> class.
    /// </summary>
    /// <param name="refresher">Instance of the <see cref="IMetadataRefresher"/> interface.</param>
    /// <param name="status">Instance of the <see cref="IRefreshStatusTracker"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="localization">Instance of the <see cref="ILocalizationManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{MetadataRefreshTask}"/> interface.</param>
    public MetadataRefreshTask(
        IMetadataRefresher refresher,
        IRefreshStatusTracker status,
        ILibraryManager libraryManager,
        ILocalizationManager localization,
        ILogger<MetadataRefreshTask> logger)
    {
        _refresher = refresher;
        _status = status;
        _libraryManager = libraryManager;
        _localization = localization;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Metadata refresh";

    /// <inheritdoc />
    public string Key => "MetadataRefresh";

    /// <inheritdoc />
    public string Description =>
        "Refreshes the metadata of the libraries selected in the Metadata refresh plugin settings. It has no schedule by default.";

    /// <inheritdoc />
    public string Category => _localization.GetLocalizedString("TasksLibraryCategory");

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // No trigger on purpose: the task only runs when the user schedules it or starts it.
        return Array.Empty<TaskTriggerInfo>();
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var options = RefreshOptionsFactory.Describe(config);

        _status.Begin(options);

        // A refresh and a library scan write the same items: do not compete with a scan.
        if (_libraryManager.IsScanRunning)
        {
            const string Reason = "Skipped because a library scan is running. It will run again on the next trigger.";
            _logger.LogInformation(Plugin.LogPrefix + Reason);
            _status.Finish(RefreshState.Skipped, Reason);
            return;
        }

        if (config.LibraryIds.Length == 0)
        {
            const string Reason = "No library is selected: choose the libraries in the Metadata refresh plugin settings.";
            _logger.LogWarning(Plugin.LogPrefix + Reason);
            _status.Finish(RefreshState.Skipped, Reason);
            return;
        }

        try
        {
            progress.Report(0);
            _logger.LogInformation(
                Plugin.LogPrefix + "Started for {Count} selected libraries. Options: {Options}",
                config.LibraryIds.Length,
                options);

            var result = await _refresher.RefreshAsync(config, progress, cancellationToken).ConfigureAwait(false);

            var message = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0} libraries refreshed, {1} failed, {2} not found.",
                result.LibrariesRefreshed,
                result.LibrariesFailed,
                result.LibrariesMissing);

            if (result.LibrariesRefreshed == 0 && (result.LibrariesFailed > 0 || result.LibrariesMissing > 0))
            {
                _logger.LogError(Plugin.LogPrefix + "Nothing was refreshed. {Message}", message);
                _status.Finish(RefreshState.Failed, message);
            }
            else
            {
                _logger.LogInformation(Plugin.LogPrefix + "Completed. {Message}", message);
                _status.Finish(RefreshState.Completed, message);
            }

            progress.Report(100);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(Plugin.LogPrefix + "Cancelled.");
            _status.Finish(RefreshState.Cancelled, "Cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Plugin.LogPrefix + "The task failed");
            _status.Finish(RefreshState.Failed, ex.Message);
        }
    }
}
