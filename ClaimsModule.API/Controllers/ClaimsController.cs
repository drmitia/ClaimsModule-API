using ClaimsModule.Application.Claims.Commands.CreateClaim;
using ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;
using ClaimsModule.Application.Claims.Queries.GetAuditLog;
using ClaimsModule.Application.Claims.Queries.GetClaimDetail;
using ClaimsModule.Application.Claims.Queries.ListClaims;
using ClaimsModule.Application.Claims.Queries.ValidateClaim;
using ClaimsModule.Domain.Enumerations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ClaimsModule.Application.Common.Models;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ListClaims(
        [FromQuery] ListClaimsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateClaim(
        [FromBody] CreateClaimCommand command,
        CancellationToken cancellationToken)
    {
        var claimId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetClaim), new { id = claimId }, claimId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaim(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClaimDetailQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/transition")]
    [ProducesResponseType(typeof(TransitionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TransitionStatus(
        Guid id,
        [FromBody] TransitionStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new TransitionClaimStatusCommand
        {
            ClaimId = id,
            NewStatus = request.NewStatus,
            Reason = request.Reason,
            ConfirmCloseWithOpenReserves = request.ConfirmCloseWithOpenReserves,
            OpenReservesJustification = request.OpenReservesJustification
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Succeeded && result.BlockingIssues.Any())
            return UnprocessableEntity(result);

        return Ok(result);
    }
    
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAuditLogQuery(id), cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateClaim(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ValidateClaimQuery(id), cancellationToken);
        return Ok(result);
    }
}

public record TransitionStatusRequest(
    ClaimStatus NewStatus,
    string? Reason,
    bool ConfirmCloseWithOpenReserves = false,
    string? OpenReservesJustification = null);