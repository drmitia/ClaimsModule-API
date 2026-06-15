using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.AddRiskObject;

public class AddRiskObjectCommandValidator : AbstractValidator<AddRiskObjectCommand>
{
    public AddRiskObjectCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty();

        RuleFor(x => x.AssetType)
            .IsInEnum()
            .WithMessage("Invalid asset type.");

        RuleFor(x => x.AssetDescription)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Asset description is required and must be under 500 characters.");

        RuleFor(x => x.DamageDescription)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.DamageDescription));

        RuleFor(x => x.AssetReference)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.AssetReference));
    }
}