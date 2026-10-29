using Jellyfin.Plugin.MetadataRefresh.Configuration;
using Jellyfin.Plugin.MetadataRefresh.Models;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;

namespace Jellyfin.Plugin.MetadataRefresh.Services;

/// <summary>
/// Translates the settings of the plugin into the options of the metadata refresh of Jellyfin,
/// following what the "Refresh metadata" dialog of the web client sends to the server.
/// </summary>
public static class RefreshOptionsFactory
{
    /// <summary>
    /// Builds the options for one refresh. Every refresh needs its own instance because the
    /// directory service caches what it reads from the file system.
    /// </summary>
    /// <param name="configuration">The plugin configuration.</param>
    /// <param name="fileSystem">Instance of the <see cref="IFileSystem"/> interface.</param>
    /// <returns>The refresh options.</returns>
    public static MetadataRefreshOptions Build(PluginConfiguration configuration, IFileSystem fileSystem)
    {
        var scanOnly = configuration.RefreshMode == RefreshMode.ScanForNewAndUpdatedFiles;
        var replaceAllMetadata = configuration.RefreshMode == RefreshMode.ReplaceAllMetadata;
        var mode = scanOnly ? MetadataRefreshMode.Default : MetadataRefreshMode.FullRefresh;

        return new MetadataRefreshOptions(new DirectoryService(fileSystem))
        {
            MetadataRefreshMode = mode,
            ImageRefreshMode = mode,
            ReplaceAllMetadata = replaceAllMetadata,
            RemoveOldMetadata = replaceAllMetadata,
            ReplaceAllImages = !scanOnly && configuration.ReplaceExistingImages,
            RegenerateTrickplay = !scanOnly && configuration.RegenerateTrickplay,
            ForceSave = !scanOnly,

            // The same as a refresh started by hand from the library menu.
            IsAutomated = false
        };
    }

    /// <summary>
    /// Describes the options in a short sentence, for the log and for the settings page.
    /// </summary>
    /// <param name="configuration">The plugin configuration.</param>
    /// <returns>The description.</returns>
    public static string Describe(PluginConfiguration configuration)
    {
        var mode = configuration.RefreshMode switch
        {
            RefreshMode.ScanForNewAndUpdatedFiles => "scan for new and updated files",
            RefreshMode.ReplaceAllMetadata => "replace all metadata",
            _ => "search for missing metadata"
        };

        if (configuration.RefreshMode == RefreshMode.ScanForNewAndUpdatedFiles)
        {
            return mode;
        }

        return mode
            + ", replace existing images: " + (configuration.ReplaceExistingImages ? "yes" : "no")
            + ", regenerate trickplay images: " + (configuration.RegenerateTrickplay ? "yes" : "no");
    }
}
