using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Links;
using LinkVault.Application.Features.Links.Commands;
using LinkVault.Application.Features.Links.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers;

[ApiController]
[Route("links")]
[Authorize]
public class LinksController : ControllerBase
{
    private readonly IMediator _mediator;

    public LinksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLinkRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new CreateLinkCommand(userId.Value, request.Url, request.Title, request.Note, request.CollectionId, request.TagIds ?? Array.Empty<Guid>()), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] LinkListQueryParams queryParams, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ListLinksQuery(
            userId.Value,
            queryParams.Query,
            queryParams.Tag,
            queryParams.Collection,
            queryParams.Favorite,
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20,
            queryParams.Sort
        ), ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new GetLinkQuery(userId.Value, id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLinkRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new UpdateLinkCommand(userId.Value, id, request.Url, request.Title, request.Note, request.CollectionId, request.TagIds ?? Array.Empty<Guid>(), request.IsFavorite), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(new DeleteLinkCommand(userId.Value, id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/favorite")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ToggleFavoriteCommand(userId.Value, id), ct);
        return Ok(result);
    }
}

public record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid>? TagIds);
public record UpdateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid>? TagIds, bool IsFavorite);

public record LinkListQueryParams(
    string? Query,
    Guid? Tag,
    Guid? Collection,
    bool? Favorite,
    int? Page,
    int? PageSize,
    string? Sort);
