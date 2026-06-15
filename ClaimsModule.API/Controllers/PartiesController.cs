using ClaimsModule.Application.Claims.Commands.AddParty;
using ClaimsModule.Application.Claims.Commands.RemoveParty;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/parties")]
public class PartiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddParty(
        Guid claimId,
        [FromBody] AddPartyCommand command,
        CancellationToken cancellationToken)
    {
        var partyCommand = command with { ClaimId = claimId };
        var partyId = await _mediator.Send(partyCommand, cancellationToken);
        return Created(string.Empty, partyId);
    }

    [HttpDelete("{partyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveParty(
        Guid claimId,
        Guid partyId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemovePartyCommand
        {
            ClaimId = claimId,
            PartyId = partyId
        }, cancellationToken);

        return NoContent();
    }
}