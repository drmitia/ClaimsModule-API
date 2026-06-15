using MediatR;

namespace ClaimsModule.Application.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand : IRequest
{
    public Guid ClaimId { get; init; }
    public Guid DocumentId { get; init; }
}