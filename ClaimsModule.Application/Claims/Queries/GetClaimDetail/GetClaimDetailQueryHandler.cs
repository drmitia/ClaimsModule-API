using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Queries.GetClaimDetail;

public class GetClaimDetailQueryHandler
    : IRequestHandler<GetClaimDetailQuery, ClaimDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;

    public GetClaimDetailQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStorageService storageService)
    {
        _context = context;
        _currentUser = currentUser;
        _storageService = storageService;
    }

    public async Task<ClaimDetailDto> Handle(
        GetClaimDetailQuery request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .Include(c => c.LossEvent)
            .Include(c => c.Parties.Where(p => p.IsActive))
            .Include(c => c.RiskObjects)
            .Include(c => c.ReserveComponents)
                .ThenInclude(r => r.History.OrderBy(h => h.ChangeSequence))
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId,
                cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        var documents = new List<ClaimDocumentDto>();
        foreach (var d in claim.Documents.Where(d => !d.IsDeleted))
        {
            var downloadUrl = await _storageService
                .GetDownloadUrlAsync(d.BlobPath, cancellationToken);

            documents.Add(new ClaimDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                DocumentName = d.DocumentName,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                UploadedAt = d.UploadedAt,
                BlobPath = d.BlobPath,
                DownloadUrl = downloadUrl
            });
        }
        
        return new ClaimDetailDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            PolicyNumber = claim.PolicyNumber,
            ClientName = claim.ClientName,
            Status = claim.Status,
            Severity = claim.Severity,
            ReportedDate = claim.ReportedDate,
            AssignedHandlerId = claim.AssignedHandlerId,
            Notes = claim.Notes,
            ClosedAt = claim.ClosedAt,
            ClosureReason = claim.ClosureReason,
            LossEvent = claim.LossEvent == null ? null : new LossEventDto
            {
                Id = claim.LossEvent.Id,
                LossDate = claim.LossEvent.LossDate,
                LossDescription = claim.LossEvent.LossDescription,
                LossLocation = claim.LossEvent.LossLocation,
                CauseOfLossCode = claim.LossEvent.CauseOfLossCode,
                EstimatedLossAmount = claim.LossEvent.EstimatedLossAmount,
                PoliceReportNumber = claim.LossEvent.PoliceReportNumber
            },
            Parties = claim.Parties.Select(p => new ClaimPartyDto
            {
                Id = p.Id,
                PartyRole = p.PartyRole,
                PartyType = p.PartyType,
                FirstName = p.FirstName,
                LastName = p.LastName,
                CompanyName = p.CompanyName,
                Email = p.Email,
                Phone = p.Phone,
                DisplayName = p.DisplayName
            }).ToList(),
            RiskObjects = claim.RiskObjects.Select(r => new ClaimRiskObjectDto
            {
                Id = r.Id,
                AssetType = r.AssetType,
                AssetDescription = r.AssetDescription,
                DamageDescription = r.DamageDescription,
                IsPrimary = r.IsPrimary,
                AssetReference = r.AssetReference
            }).ToList(),
            ReserveComponents = claim.ReserveComponents.Select(rc => new ReserveComponentDto
            {
                Id = rc.Id,
                Component = rc.Component,
                CurrentAmount = rc.CurrentAmount,
                Notes = rc.Notes,
                History = rc.History.Select(h => new ReserveHistoryDto
                {
                    Id = h.Id,
                    TransactionType = h.TransactionType,
                    Amount = h.Amount,
                    PreviousBalance = h.PreviousBalance,
                    NewBalance = h.NewBalance,
                    ApprovalStatus = h.ApprovalStatus,
                    ChangeReason = h.ChangeReason,
                    RejectionReason = h.RejectionReason,
                    CreatedAt = h.CreatedAt,
                    SubmittedByUserId = h.SubmittedByUserId,
                    ApprovedByUserId = h.ApprovedByUserId
                }).ToList()
            }).ToList(),
            Documents = documents
        };
    }
}