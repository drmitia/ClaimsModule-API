using ClaimsModule.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence;

public class ClaimNumberGenerator : IClaimNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public ClaimNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var result = await _context.Database
            .SqlQueryRaw<long>("SELECT NEXT VALUE FOR dbo.ClaimNumberSequence")
            .ToListAsync(cancellationToken);

        var sequenceValue = result.First();
        var year = DateTime.UtcNow.Year;

        return $"CLM-{year}-{sequenceValue:D7}";
    }
}