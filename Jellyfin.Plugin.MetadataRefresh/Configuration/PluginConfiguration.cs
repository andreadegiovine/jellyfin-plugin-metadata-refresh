using System;
using Jellyfin.Plugin.MetadataRefresh.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.MetadataRefresh.Configuration;

/// <summary>
/// Plugin configuration, edited in the settings page of the plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        LibraryIds = Array.Empty<string>();
        RefreshMode = RefreshMode.SearchForMissingMetadata;
        ReplaceExistingImages = false;
        RegenerateTrickplay = false;
    }

    /// <summary>
    /// Gets or sets the ids of the libraries whose metadata is refreshed. When it is empty the
    /// task does nothing.
    /// </summary>
    public string[] LibraryIds { get; set; }

    /// <summary>
    /// Gets or sets what the refresh does.
    /// </summary>
    public RefreshMode RefreshMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the existing images are replaced with the ones
    /// found by the providers. It is ignored when <see cref="RefreshMode"/> only scans for new and
    /// updated files.
    /// </summary>
    public bool ReplaceExistingImages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the existing trickplay images are generated
    /// again. It is ignored when <see cref="RefreshMode"/> only scans for new and updated files.
    /// </summary>
    public bool RegenerateTrickplay { get; set; }
}
