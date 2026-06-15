using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Queries.GetAuditLog;

public class GetAuditLogQueryHandler
    : IRequestHandler<GetAuditLogQuery, List<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AuditLogDto>> Handle(
        GetAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        // Verify claim exists and belongs to this organisation
        var claimExists = await _context.Claims
            .AnyAsync(c =>
                    c.Id == request.ClaimId &&
                    c.OrganisationId == _currentUser.OrganisationId,
                cancellationToken);

        if (!claimExists)
            throw new ClaimNotFoundException(request.ClaimId);

        var logs = await _context.ClaimAuditLogs
            .Where(l => l.ClaimId == request.ClaimId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new AuditLogDto
            {
                AuditLogId = l.AuditLogId,
                EventType = l.EventType,
                Description = l.Description,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                CreatedAt = l.CreatedAt,
                CreatedByUserId = l.CreatedByUserId
            })
            .ToListAsync(cancellationToken);

        return logs;
    }
}