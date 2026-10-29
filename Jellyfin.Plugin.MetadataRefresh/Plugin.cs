using System;
using System.Collections.Generic;
using Jellyfin.Plugin.MetadataRefresh.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MetadataRefresh;

/// <summary>
/// Main plugin class. The plugin contributes a scheduled task
/// (<see cref="ScheduledTasks.MetadataRefreshTask"/>), a settings page and the status endpoints
/// used by that page to show the progress of the refresh.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Prefix prepended to every log message of this plugin, to make it easy to find in the server log.
    /// </summary>
    public const string LogPrefix = "Metadata refresh: ";

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "Metadata refresh";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("0d4b7a2f-1d0f-4285-83ac-fd51b62ee4ad");

    /// <inheritdoc />
    public override string Description =>
        "Refreshes the metadata of the selected libraries with a scheduled task.";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = Name,
            DisplayName = Name,
            EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html",
            EnableInMainMenu = true,
            MenuSection = "server",
            MenuIcon = "refresh"
        };
    }
}
