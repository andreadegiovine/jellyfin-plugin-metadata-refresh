using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MetadataRefresh.Configuration;
using Jellyfin.Plugin.MetadataRefresh.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MetadataRefresh.Services;

/// <inheritdoc />
public sealed class MetadataRefresher : IMetadataRefresher
{
    private readonly ILibraryManager _libraryManager;
    private readonly IFileSystem _fileSystem;
    private readonly IRefreshStatusTracker _status;
    private readonly ILogger<MetadataRefresher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRefresher"/> class.
    /// </summary>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="fileSystem">Instance of the <see cref="IFileSystem"/> interface.</param>
    /// <param name="status">Instance of the <see cref="IRefreshStatusTracker"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{MetadataRefresher}"/> interface.</param>
    public MetadataRefresher(
        ILibraryManager libraryManager,
        IFileSystem fileSystem,
        IRefreshStatusTracker status,
        ILogger<MetadataRefresher> logger)
    {
        _libraryManager = libraryManager;
        _fileSystem = fileSystem;
        _status = status;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RefreshResult> RefreshAsync(
        PluginConfiguration configuration,
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        var result = new RefreshResult();
        var plans = ResolveLibraries(configuration, result);
        _status.SetLibrariesTotal(plans.Count);

        // Every folder of every library has the same weight in the overall progress: the real
        // work is in the children of the folders, which Jellyfin reports as a percentage.
        var totalUnits = Math.Max(1, plans.Sum(p => Math.Max(1, p.Folders.Count)));
        var unitsDone = 0;
        var highest = 0.0;
        var progressLock = new object();

        void Report(double percent)
        {
            lock (progressLock)
            {
                highest = Math.Max(highest, Math.Min(percent, 100));
                progress.Report(highest);
                _status.ReportProgress(highest);
            }
        }

        for (var i = 0; i < plans.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var plan = plans[i];
            _status.SetCurrentLibrary(plan.Library.Name, i + 1);
            _logger.LogInformation(
                Plugin.LogPrefix + "Refreshing library '{Library}' ({Index}/{Total})",
                plan.Library.Name,
                i + 1,
                plans.Count);

            try
            {
                var options = RefreshOptionsFactory.Build(configuration, _fileSystem);

                // The same steps Jellyfin runs when a library is refreshed from the web client:
                // the library itself, then every folder of the library and all their children.
                await plan.Library.RefreshMetadata(options, cancellationToken).ConfigureAwait(false);

                foreach (var folder in plan.Folders)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var unitStart = unitsDone;
                    var folderProgress = new Progress<double>(
                        percent => Report((unitStart + (Math.Clamp(percent, 0, 100) / 100.0)) / totalUnits * 100.0));

                    await folder.RefreshMetadata(options, cancellationToken).ConfigureAwait(false);
                    await folder.ValidateChildren(folderProgress, options, cancellationToken: cancellationToken).ConfigureAwait(false);

                    unitsDone++;
                    Report((double)unitsDone / totalUnits * 100.0);
                }

                if (plan.Folders.Count == 0)
                {
                    unitsDone++;
                    Report((double)unitsDone / totalUnits * 100.0);
                }

                result.LibrariesRefreshed++;
                _logger.LogInformation(Plugin.LogPrefix + "Library '{Library}' refreshed", plan.Library.Name);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // A failing library must not stop the others.
                result.LibrariesFailed++;
                _logger.LogError(ex, Plugin.LogPrefix + "Refreshing library '{Library}' failed", plan.Library.Name);

                // Skip the units of the failed library so the progress keeps moving.
                var consumed = unitsDone;
                unitsDone = plans.Take(i + 1).Sum(p => Math.Max(1, p.Folders.Count));
                if (unitsDone > consumed)
                {
                    Report((double)unitsDone / totalUnits * 100.0);
                }
            }
        }

        return result;
    }

    private List<LibraryPlan> ResolveLibraries(PluginConfiguration configuration, RefreshResult result)
    {
        var plans = new List<LibraryPlan>();

        foreach (var rawId in configuration.LibraryIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(rawId, out var id) || _libraryManager.GetItemById(id) is not CollectionFolder library)
            {
                result.LibrariesMissing++;
                _logger.LogWarning(Plugin.LogPrefix + "The selected library '{LibraryId}' no longer exists: it is skipped", rawId);
                continue;
            }

            plans.Add(new LibraryPlan(library, library.GetPhysicalFolders().ToList()));
        }

        return plans;
    }

    private sealed record LibraryPlan(CollectionFolder Library, IReadOnlyList<Folder> Folders);
}
