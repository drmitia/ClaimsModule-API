using FluentValidation;

namespace ClaimsModule.Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty();

        RuleFor(x => x.DocumentName)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Document name is required and must be under 255 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => AllowedMimeTypes.Contains(ct))
            .WithMessage($"File type not allowed. Allowed types: {string.Join(", ", AllowedMimeTypes)}");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(20 * 1024 * 1024) // 20MB
            .WithMessage("File size must be between 1 byte and 20MB.");

        RuleFor(x => x.DocumentType)
            .IsInEnum()
            .WithMessage("Invalid document type.");
    }
}