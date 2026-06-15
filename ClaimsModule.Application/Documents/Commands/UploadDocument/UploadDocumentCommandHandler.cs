using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public UploadDocumentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStorageService storageService,
        IAuditLogService auditLog,
        ICorrelationIdService correlationId)
    {
        _context = context;
        _currentUser = currentUser;
        _storageService = storageService;
        _auditLog = auditLog;
        _correlationId = correlationId;
    }

    public async Task<Guid> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.Claims
            .FirstOrDefaultAsync(c =>
                c.Id == request.ClaimId &&
                c.OrganisationId == _currentUser.OrganisationId &&
                !c.IsDeleted, cancellationToken)
            ?? throw new ClaimNotFoundException(request.ClaimId);

        var blobPath = await _storageService.UploadAsync(
            request.FileStream,
            request.DocumentName,
            request.ContentType,
            _currentUser.OrganisationId,
            claim.Id,
            cancellationToken);

        var document = new ClaimDocument
        {
            ClaimId = claim.Id,
            DocumentType = request.DocumentType,
            DocumentName = request.DocumentName,
            BlobPath = blobPath,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedByUserId = _currentUser.UserId,
            OrganisationId = _currentUser.OrganisationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        _context.ClaimDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: claim.Id,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.DocumentUploaded,
            description: $"Document '{request.DocumentName}' uploaded",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: document.Id,
            relatedEntityType: "ClaimDocument",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);

        return document.Id;
    }
}