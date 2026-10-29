using Jellyfin.Plugin.MetadataRefresh.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.MetadataRefresh;

/// <summary>
/// Registers the plugin's supporting services. The scheduled task itself must not be registered
/// here: Jellyfin discovers every <c>IScheduledTask</c> in the loaded assemblies and builds it
/// through the DI container, which is why its dependencies are registered here.
/// </summary>
public sealed class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IRefreshStatusTracker, RefreshStatusTracker>();
        serviceCollection.AddSingleton<IMetadataRefresher, MetadataRefresher>();
    }
}
