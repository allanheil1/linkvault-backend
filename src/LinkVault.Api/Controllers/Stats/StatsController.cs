using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Stats.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers.Stats;

/// <summary>
/// Endpoints that expose aggregate usage statistics for the authenticated user.
/// </summary>
[ApiController]
[Route("stats")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns high-level counters for links, favorites, tags, and collections.
    /// </summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>Overview counters.</returns>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(StatsOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new StatsOverviewQuery(userId.Value), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns the most frequently used tags.
    /// </summary>
    /// <param name="limit">Maximum number of tags to return.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>List of popular tags ordered by usage.</returns>
    [HttpGet("popular-tags")]
    [ProducesResponseType(typeof(IEnumerable<PopularTagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PopularTags([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new PopularTagsQuery(userId.Value, limit), ct);
        return Ok(result);
    }
}
