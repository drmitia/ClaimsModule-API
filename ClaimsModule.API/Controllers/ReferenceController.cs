using ClaimsModule.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferenceController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReferenceController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("cause-of-loss-codes")]
    public async Task<IActionResult> GetCauseOfLossCodes(
        CancellationToken cancellationToken)
    {
        var codes = await _context.CauseOfLossCodes
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new { c.Code, c.Name, c.PerilCategory })
            .ToListAsync(cancellationToken);

        return Ok(codes);
    }

    [HttpGet("policies/search")]
    public async Task<IActionResult> SearchPolicies(
        [FromQuery] string term,
        CancellationToken cancellationToken)
    {
        var policies = await _context.Policies
            .Where(p => p.PolicyNumber.Contains(term)
                        || p.ClientName.Contains(term))
            .Select(p => new
            {
                p.PolicyId,
                p.PolicyNumber,
                p.ClientName,
                p.Status,
                p.EffectiveDate,
                p.ExpirationDate
            })
            .Take(10)
            .ToListAsync(cancellationToken);

        return Ok(policies);
    }
}