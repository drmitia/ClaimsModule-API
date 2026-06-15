using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Documents.Commands.UploadDocument;

public record UploadDocumentCommand : IRequest<Guid>
{
    public Guid ClaimId { get; init; }
    public DocumentType DocumentType { get; init; }
    public string DocumentName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public Stream FileStream { get; init; } = Stream.Null;
}