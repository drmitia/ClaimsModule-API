using ClaimsModule.Application.Common;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;
    private readonly IAuditLogService _auditLog;
    private readonly ICorrelationIdService _correlationId;

    public DeleteDocumentCommandHandler(
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

    public async Task Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _context.ClaimDocuments
            .FirstOrDefaultAsync(d =>
                d.Id == request.DocumentId &&
                d.ClaimId == request.ClaimId &&
                d.OrganisationId == _currentUser.OrganisationId &&
                !d.IsDeleted, cancellationToken)
            ?? throw new DomainException(
                $"Document with ID '{request.DocumentId}' was not found.");

        // Delete from storage
        await _storageService.DeleteAsync(document.BlobPath, cancellationToken);

        // Soft delete the record
        document.IsDeleted = true;
        document.DeletedAt = DateTimeOffset.UtcNow;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        document.ModifiedByUserId = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.LogAsync(
            claimId: request.ClaimId,
            organisationId: _currentUser.OrganisationId,
            eventType: AuditEventTypes.DocumentDeleted,
            description: $"Document '{document.DocumentName}' deleted",
            createdByUserId: _currentUser.UserId,
            relatedEntityId: document.Id,
            relatedEntityType: "ClaimDocument",
            correlationId: _correlationId.CorrelationId,
            cancellationToken: cancellationToken);
    }
}