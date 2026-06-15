using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.CreateClaim;

public class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IClaimNumberGenerator _claimNumberGenerator;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public CreateClaimCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IClaimNumberGenerator claimNumberGenerator,
        IAuditLogService auditLog,
        ICorrelationIdService correlationId)
    {
        _context = context;
        _currentUser = currentUser;
        _claimNumberGenerator = claimNumberGenerator;
        _auditLog = auditLog;
        _correlationId = correlationId;
    }

    public async Task<Guid> Handle(
        CreateClaimCommand request,
        CancellationToken cancellationToken)
    {
        var claimNumber = await _claimNumberGenerator.GenerateAsync(cancellationToken);

        var claim = new Claim
        {
            ClaimNumber = claimNumber,
            PolicyId = request.PolicyId,
            PolicyNumber = request.PolicyNumber,
            ClientName = request.ClientName,
            Status = ClaimStatus.Draft,
            ReportedDate = DateTimeOffset.UtcNow,
            OrganisationId = _currentUser.OrganisationId,
            CreatedByUserId = _currentUser.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            AssignedHandlerId = _currentUser.UserId,
            Notes = request.Notes,
            LossEvent = new LossEvent
            {
                LossDate = request.LossDate,
                LossDescription = request.LossDescription,
                LossLocation = request.LossLocation,
                CauseOfLossCode = request.CauseOfLossCode,
                EstimatedLossAmount = request.EstimatedLossAmount,
                ReportDate = DateTimeOffset.UtcNow,
                PoliceReportNumber = request.PoliceReportNumber,
                OrganisationId = _currentUser.OrganisationId,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        };

        // Add initial reserve if provided
        if (request.InitialReserveAmount.HasValue && request.InitialReserveComponent.HasValue)
        {
            var reserveComponent = new ClaimReserveComponent
            {
                ClaimId = claim.Id,
                Component = request.InitialReserveComponent.Value,
                CurrentAmount = request.InitialReserveAmount.Value,
                OrganisationId = _currentUser.OrganisationId,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            reserveComponent.History.Add(new ReserveHistory
            {
                ClaimId = claim.Id,
                TransactionType = TransactionType.Add,
                Amount = request.InitialReserveAmount.Value,
                PreviousBalance = 0,
                NewBalance = request.InitialReserveAmount.Value,
                ApprovalStatus = ApprovalStatus.AutoApproved,
                ChangeReason = "Initial reserve set at FNOL",
                ChangeSequence = 1,
                IdempotencyKey = $"Reserve:{reserveComponent.Id}:Change:1",
                SubmittedByUserId = _currentUser.UserId,
                OrganisationId = _currentUser.OrganisationId,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            claim.ReserveComponents.Add(reserveComponent);
        }

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync(cancellationToken);

        // Policy unknown warning
        if (claim.PolicyId is null)
        {
            await _auditLog.LogAsync(
                claimId: claim.Id,
                organisationId: _currentUser.OrganisationId,
                eventType: AuditEventTypes.PolicyUnknown,
                description: "Warning: Policy Unknown — claim created without a linked policy",
                createdByUserId: _currentUser.UserId,
                correlationId: _correlationId.CorrelationId,
                cancellationToken: cancellationToken);
        }
        
        await _auditLog.LogAsync(
            claimId: claim.Id,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.ClaimCreated,
            description: $"Claim {claimNumber} created via FNOL",
            createdByUserId: _currentUser.UserId,
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);

        return claim.Id;
    }
}