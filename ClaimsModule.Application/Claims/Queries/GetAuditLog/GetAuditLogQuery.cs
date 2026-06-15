using ClaimsModule.Application.Common.Models;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetAuditLog;

public record GetAuditLogQuery(Guid ClaimId) : IRequest<List<AuditLogDto>>;