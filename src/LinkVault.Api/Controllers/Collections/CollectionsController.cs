using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Collections;
using LinkVault.Application.Features.Collections.Commands;
using LinkVault.Application.Features.Collections.Queries;
using LinkVault.Application.Features.Links.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers.Collections;

[ApiController]
[Route("collections")]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CollectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ListCollectionsQuery(userId.Value), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new GetCollectionQuery(userId.Value, id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CollectionRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new CreateCollectionCommand(userId.Value, request.Name), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CollectionRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new UpdateCollectionCommand(userId.Value, id, request.Name), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(new DeleteCollectionCommand(userId.Value, id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/links")]
    [ProducesResponseType(typeof(PagedResult<LinkVault.Application.Features.Links.LinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Links(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ListCollectionLinksQuery(userId.Value, id, page, pageSize), ct);
        return Ok(result);
    }
}

public record CollectionRequest(string Name);
