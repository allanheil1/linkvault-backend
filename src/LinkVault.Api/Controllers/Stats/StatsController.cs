using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Stats.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers.Stats;

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

    [HttpGet("overview")]
    [ProducesResponseType(typeof(StatsOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new StatsOverviewQuery(userId.Value), ct);
        return Ok(result);
    }

    [HttpGet("popular-tags")]
    [ProducesResponseType(typeof(IEnumerable<PopularTagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PopularTags([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new PopularTagsQuery(userId.Value, limit), ct);
        return Ok(result);
    }
}
