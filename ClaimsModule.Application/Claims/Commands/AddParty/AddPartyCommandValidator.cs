using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.AddParty;

public class AddPartyCommandValidator : AbstractValidator<AddPartyCommand>
{
    public AddPartyCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty();

        RuleFor(x => x.PartyRole)
            .IsInEnum()
            .WithMessage("Invalid party role.");

        RuleFor(x => x.PartyType)
            .IsInEnum()
            .WithMessage("Invalid party type.");

        // Person validation
        When(x => x.PartyType == Domain.Enumerations.PartyType.Person, () =>
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("First name is required for a person.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Last name is required for a person.");
        });

        // Company validation
        When(x => x.PartyType == Domain.Enumerations.PartyType.Company, () =>
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("Company name is required for a company.");
        });

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Invalid email address.");

        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}