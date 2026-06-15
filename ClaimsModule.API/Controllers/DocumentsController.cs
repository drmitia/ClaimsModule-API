using ClaimsModule.Application.Documents.Commands.DeleteDocument;
using ClaimsModule.Application.Documents.Commands.UploadDocument;
using ClaimsModule.Domain.Enumerations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadDocument(
        Guid claimId,
        IFormFile file,
        [FromForm] DocumentType documentType,
        CancellationToken cancellationToken)
    {
        var command = new UploadDocumentCommand
        {
            ClaimId = claimId,
            DocumentType = documentType,
            DocumentName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            FileStream = file.OpenReadStream()
        };

        var documentId = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, documentId);
    }

    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        Guid claimId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDocumentCommand
        {
            ClaimId = claimId,
            DocumentId = documentId
        }, cancellationToken);

        return NoContent();
    }
}