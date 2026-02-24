using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Tags;
using LinkVault.Application.Features.Tags.Commands;
using LinkVault.Application.Features.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkVault.Api.Controllers.Tags;

/// <summary>
/// Endpoints for managing user tags.
/// </summary>
[ApiController]
[Route("tags")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all tags for the authenticated user.
    /// </summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>List of tags.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ListTagsQuery(userId.Value), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a tag by id.
    /// </summary>
    /// <param name="id">Tag identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The tag when found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new GetTagQuery(userId.Value, id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="request">Tag payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The created tag.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] TagRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new CreateTagCommand(userId.Value, request.Name), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing tag.
    /// </summary>
    /// <param name="id">Tag identifier.</param>
    /// <param name="request">Updated tag payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The updated tag.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TagRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new UpdateTagCommand(userId.Value, id, request.Name), ct);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a tag by id.
    /// </summary>
    /// <param name="id">Tag identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(new DeleteTagCommand(userId.Value, id), ct);
        return NoContent();
    }
}

/// <summary>
/// Payload used to create or update a tag.
/// </summary>
/// <param name="Name">Tag name.</param>
public record TagRequest(string Name);
