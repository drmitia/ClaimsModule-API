using ClaimsModule.Application.Reserves.Commands.ApproveReserve;
using ClaimsModule.Application.Reserves.Commands.CreateReserve;
using ClaimsModule.Application.Reserves.Commands.RejectReserve;
using ClaimsModule.Application.Reserves.Commands.RetractReserve;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/reserves")]
public class ReservesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateReserve(
        Guid claimId,
        [FromBody] CreateReserveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReserveCommand
        {
            ClaimId = claimId,
            Component = request.Component,
            Amount = request.Amount,
            ChangeReason = request.ChangeReason
        };

        var historyId = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, historyId);
    }

    [HttpPost("{reserveHistoryId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveReserve(
        Guid claimId,
        Guid reserveHistoryId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveReserveCommand
        {
            ClaimId = claimId,
            ReserveHistoryId = reserveHistoryId
        }, cancellationToken);

        return NoContent();
    }

    [HttpPost("{reserveHistoryId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectReserve(
        Guid claimId,
        Guid reserveHistoryId,
        [FromBody] RejectReserveRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectReserveCommand
        {
            ClaimId = claimId,
            ReserveHistoryId = reserveHistoryId,
            RejectionReason = request.RejectionReason
        }, cancellationToken);

        return NoContent();
    }

    [HttpPost("{reserveHistoryId:guid}/retract")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RetractReserve(
        Guid claimId,
        Guid reserveHistoryId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RetractReserveCommand
        {
            ClaimId = claimId,
            ReserveHistoryId = reserveHistoryId
        }, cancellationToken);

        return NoContent();
    }
}

public record CreateReserveRequest(
    ClaimsModule.Domain.Enumerations.ReserveComponent Component,
    decimal Amount,
    string ChangeReason);

public record RejectReserveRequest(string RejectionReason);