using ClaimsModule.Application.Claims.Commands.AddRiskObject;
using ClaimsModule.Application.Claims.Commands.RemoveRiskObject;
using ClaimsModule.Domain.Enumerations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/risk-objects")]
public class RiskObjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RiskObjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddRiskObject(
        Guid claimId,
        [FromBody] AddRiskObjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddRiskObjectCommand
        {
            ClaimId = claimId,
            AssetType = request.AssetType,
            AssetDescription = request.AssetDescription,
            DamageDescription = request.DamageDescription,
            IsPrimary = request.IsPrimary,
            AssetReference = request.AssetReference
        };

        var riskObjectId = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, riskObjectId);
    }

    [HttpDelete("{riskObjectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRiskObject(
        Guid claimId,
        Guid riskObjectId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveRiskObjectCommand
        {
            ClaimId = claimId,
            RiskObjectId = riskObjectId
        }, cancellationToken);

        return NoContent();
    }
}

public record AddRiskObjectRequest(
    AssetType AssetType,
    string AssetDescription,
    string? DamageDescription,
    bool IsPrimary = false,
    string? AssetReference = null);