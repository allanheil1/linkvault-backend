using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Links;
using LinkVault.Application.Features.Links.Commands;
using LinkVault.Application.Features.Links.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers;

/// <summary>
/// Endpoints for managing user links.
/// </summary>
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

    /// <summary>
    /// Creates a new link for the authenticated user.
    /// </summary>
    /// <param name="request">Link creation payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The created link.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateLinkRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new CreateLinkCommand(userId.Value, request.Url, request.Title, request.Note, request.CollectionId, request.TagIds ?? Array.Empty<Guid>()), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Returns a paged list of links with optional filters and sorting.
    /// </summary>
    /// <param name="queryParams">Filtering, paging, and sorting options.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>Paged list of links.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Returns a single link by id.
    /// </summary>
    /// <param name="id">Link identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The link when found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new GetLinkQuery(userId.Value, id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Updates an existing link.
    /// </summary>
    /// <param name="id">Link identifier.</param>
    /// <param name="request">Updated link payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The updated link.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLinkRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new UpdateLinkCommand(userId.Value, id, request.Url, request.Title, request.Note, request.CollectionId, request.TagIds ?? Array.Empty<Guid>(), request.IsFavorite), ct);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a link by id.
    /// </summary>
    /// <param name="id">Link identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(new DeleteLinkCommand(userId.Value, id), ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles favorite status for a link.
    /// </summary>
    /// <param name="id">Link identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The updated link.</returns>
    [HttpPatch("{id:guid}/favorite")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ToggleFavoriteCommand(userId.Value, id), ct);
        return Ok(result);
    }
}

/// <summary>
/// Payload used to create a link.
/// </summary>
/// <param name="Url">Link URL.</param>
/// <param name="Title">Link title.</param>
/// <param name="Note">Optional note.</param>
/// <param name="CollectionId">Optional collection id.</param>
/// <param name="TagIds">Optional tag ids.</param>
public record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid>? TagIds);

/// <summary>
/// Payload used to update a link.
/// </summary>
/// <param name="Url">Link URL.</param>
/// <param name="Title">Link title.</param>
/// <param name="Note">Optional note.</param>
/// <param name="CollectionId">Optional collection id.</param>
/// <param name="TagIds">Optional tag ids.</param>
/// <param name="IsFavorite">Favorite status to persist.</param>
public record UpdateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid>? TagIds, bool IsFavorite);

/// <summary>
/// Query parameters used to filter and paginate links.
/// </summary>
/// <param name="Query">Search term for title, note, or URL.</param>
/// <param name="Tag">Tag id filter.</param>
/// <param name="Collection">Collection id filter.</param>
/// <param name="Favorite">Favorite filter.</param>
/// <param name="Page">Page number (starts at 1).</param>
/// <param name="PageSize">Page size (1-100).</param>
/// <param name="Sort">Sort expression: createdat, -createdat, title, -title.</param>
public record LinkListQueryParams(
    string? Query,
    Guid? Tag,
    Guid? Collection,
    bool? Favorite,
    int? Page,
    int? PageSize,
    string? Sort);
