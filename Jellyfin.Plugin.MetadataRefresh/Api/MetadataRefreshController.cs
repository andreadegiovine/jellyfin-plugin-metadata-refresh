using System.Net.Mime;
using Jellyfin.Plugin.MetadataRefresh.Models;
using Jellyfin.Plugin.MetadataRefresh.ScheduledTasks;
using Jellyfin.Plugin.MetadataRefresh.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.MetadataRefresh.Api;

/// <summary>
/// Endpoints used by the settings page of the plugin to start and stop the refresh and to read its
/// progress. Only administrators can call them.
/// </summary>
[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("MetadataRefresh")]
[Produces(MediaTypeNames.Application.Json)]
public class MetadataRefreshController : ControllerBase
{
    private readonly ITaskManager _taskManager;
    private readonly IRefreshStatusTracker _status;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRefreshController"/> class.
    /// </summary>
    /// <param name="taskManager">Instance of the <see cref="ITaskManager"/> interface.</param>
    /// <param name="status">Instance of the <see cref="IRefreshStatusTracker"/> interface.</param>
    public MetadataRefreshController(ITaskManager taskManager, IRefreshStatusTracker status)
    {
        _taskManager = taskManager;
        _status = status;
    }

    /// <summary>
    /// Gets the progress of the refresh.
    /// </summary>
    /// <response code="200">The status of the refresh.</response>
    /// <returns>The status of the refresh.</returns>
    [HttpGet("Status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<RefreshStatus> GetStatus()
    {
        return _status.GetSnapshot();
    }

    /// <summary>
    /// Starts the refresh, unless it is already running.
    /// </summary>
    /// <response code="204">The refresh was queued.</response>
    /// <returns>An empty response.</returns>
    [HttpPost("Start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Start()
    {
        _taskManager.QueueIfNotRunning<MetadataRefreshTask>();
        return NoContent();
    }

    /// <summary>
    /// Stops the refresh if it is running.
    /// </summary>
    /// <response code="204">The refresh was asked to stop.</response>
    /// <returns>An empty response.</returns>
    [HttpPost("Stop")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Stop()
    {
        _taskManager.CancelIfRunning<MetadataRefreshTask>();
        return NoContent();
    }
}
