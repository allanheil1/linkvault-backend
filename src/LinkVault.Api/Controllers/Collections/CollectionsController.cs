using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Collections;
using LinkVault.Application.Features.Collections.Commands;
using LinkVault.Application.Features.Collections.Queries;
using LinkVault.Application.Features.Links.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers.Collections;

/// <summary>
/// Endpoints for managing user collections.
/// </summary>
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

    /// <summary>
    /// Returns all collections for the authenticated user.
    /// </summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>List of collections.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ListCollectionsQuery(userId.Value), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a collection by id.
    /// </summary>
    /// <param name="id">Collection identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The collection when found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new GetCollectionQuery(userId.Value, id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a new collection.
    /// </summary>
    /// <param name="request">Collection payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The created collection.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CollectionRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new CreateCollectionCommand(userId.Value, request.Name), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing collection.
    /// </summary>
    /// <param name="id">Collection identifier.</param>
    /// <param name="request">Updated collection payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The updated collection.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CollectionRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new UpdateCollectionCommand(userId.Value, id, request.Name), ct);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a collection by id.
    /// </summary>
    /// <param name="id">Collection identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(new DeleteCollectionCommand(userId.Value, id), ct);
        return NoContent();
    }

    /// <summary>
    /// Returns paged links that belong to a specific collection.
    /// </summary>
    /// <param name="id">Collection identifier.</param>
    /// <param name="page">Page number (starts at 1).</param>
    /// <param name="pageSize">Page size (1-100).</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>Paged list of links in the collection.</returns>
    [HttpGet("{id:guid}/links")]
    [ProducesResponseType(typeof(PagedResult<LinkVault.Application.Features.Links.LinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Links(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ListCollectionLinksQuery(userId.Value, id, page, pageSize), ct);
        return Ok(result);
    }
}

/// <summary>
/// Payload used to create or update a collection.
/// </summary>
/// <param name="Name">Collection name.</param>
public record CollectionRequest(string Name);
