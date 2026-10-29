namespace Jellyfin.Plugin.MetadataRefresh.Models;

/// <summary>
/// What the refresh does, with the same meaning as the modes of the "Refresh metadata" dialog of
/// Jellyfin.
/// </summary>
public enum RefreshMode
{
    /// <summary>
    /// Scans for new and updated files and fetches metadata only for what is new or changed.
    /// </summary>
    ScanForNewAndUpdatedFiles = 0,

    /// <summary>
    /// Queries the metadata providers to fill in what is missing, keeping the existing metadata.
    /// </summary>
    SearchForMissingMetadata = 1,

    /// <summary>
    /// Queries the metadata providers and replaces all the existing metadata.
    /// </summary>
    ReplaceAllMetadata = 2
}
