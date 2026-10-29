# Metadata refresh

A Jellyfin **12** plugin that adds a scheduled task, **Metadata refresh**, which refreshes the
metadata of the libraries you choose, with the same options as the *Refresh metadata* dialog of
Jellyfin, and shows its progress in the Dashboard.

The task is under *Dashboard → Scheduled Tasks → Library*. **It has no schedule by default**: add
the trigger you want in *Scheduled Tasks*, or run it by hand. Its options are in the plugin settings
page, which has its own **Metadata refresh** entry in the Dashboard sidebar (*Server* section).

## Settings

| Option | Default | Meaning |
| --- | --- | --- |
| Libraries | none selected | The libraries whose metadata is refreshed. When none is selected the task does nothing. |
| Refresh mode | Search for missing metadata | *Scan for new and updated files*: fetches metadata only for what is new or changed. *Search for missing metadata*: queries the providers and fills in what is missing, keeping the existing metadata. *Replace all metadata*: queries the providers and overwrites all the existing metadata. |
| Replace existing images | off | Downloads the images again and replaces the existing ones. Not used by *Scan for new and updated files*. |
| Replace existing trickplay images | off | Generates the trickplay (seek preview) images again. Not used by *Scan for new and updated files*. |

The modes and checkboxes are the ones of the *Refresh metadata* dialog of the web client, and the
task asks the server for the same operation: the library, every folder of the library and all their
children are refreshed.

## Progress

- **Scheduled Tasks page**: the standard progress bar of the task.
- **Plugin settings page**: state (idle, running, completed, cancelled, failed, skipped), a progress
  bar, the library being refreshed, the options of the run and the start and end time. The page
  also has **Run now** and **Stop** buttons (they use the saved settings).

The status is kept in memory: it starts as *Idle* when the server starts.

## Requirements

- Jellyfin **12.0 or newer** (built against 12.1).

## Installation

### From the plugin repository (recommended)

1. In Jellyfin go to **Dashboard → Plugins → Repositories** and click **+**.
2. Enter a name (e.g. `Metadata refresh`) and this URL:

   ```
   https://andreadegiovine.github.io/jellyfin-plugin-metadata-refresh/repository.json
   ```

3. Open **Dashboard → Plugins → Catalog**, find **Metadata refresh** and click **Install**.
4. **Restart Jellyfin.**

### Manual installation

1. Download the latest `metadata-refresh_<version>.zip` from the
   [Releases](https://github.com/andreadegiovine/jellyfin-plugin-metadata-refresh/releases) page.
2. Extract it into a new folder inside your Jellyfin `plugins` directory.
3. **Restart Jellyfin.**

## What the task does

1. Reads the libraries selected in the settings. A selected library that no longer exists is logged
   and skipped.
2. Skips the run if a library scan is running, so the two do not write the same items.
3. Refreshes the libraries one after the other. For each library it refreshes the library itself and
   then every folder of the library together with all its children, exactly as Jellyfin does when a
   library is refreshed from the web client.
4. Reports the progress while it works. A library that fails is logged and does not stop the
   others. The task can be cancelled from *Scheduled Tasks* or with **Stop**.

A refresh in *Replace all metadata* mode, or with *Replace existing images*, overwrites data and
sends many requests to the metadata providers: on large libraries it takes a long time and can hit
the rate limits of the providers. Start with a single library.

## Troubleshooting

Every log line from this plugin starts with `Metadata refresh:`, so you can filter the Jellyfin
log (**Dashboard → Logs**) on that text. The log lists the options of the run, each library as it
starts and ends, and every error.

If you open an issue, please include the log lines starting with `Metadata refresh:`.

## Building from source

Requires the .NET 10 SDK.

```bash
dotnet restore
dotnet build -c Release
```

The output is `Jellyfin.Plugin.MetadataRefresh/bin/Release/net10.0/Jellyfin.Plugin.MetadataRefresh.dll`.

## Releasing

Publishing a GitHub Release runs `.github/workflows/publish.yaml`, which builds the plugin, uploads
the zip with its checksums to the release, and regenerates the plugin repository manifest
(`repository.json`) on the `gh-pages` branch. Tags use the format `vX.Y.Z.W`.

## License

Apache License 2.0, see [LICENSE](LICENSE).
