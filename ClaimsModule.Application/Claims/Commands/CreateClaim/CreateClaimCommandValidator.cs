using ClaimsModule.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Claims.Commands.CreateClaim;

public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.LossDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Loss date cannot be in the future.");

        RuleFor(x => x.LossDescription)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(2000)
            .WithMessage("Loss description must be between 10 and 2000 characters.");

        RuleFor(x => x.CauseOfLossCode)
            .NotEmpty()
            .WithMessage("Cause of loss code is required.")
            .MustAsync(async (code, ct) =>
                await context.CauseOfLossCodes
                    .AnyAsync(c => c.Code == code && c.IsActive, ct))
            .WithMessage("Invalid or inactive cause of loss code.");

        RuleFor(x => x.EstimatedLossAmount)
            .GreaterThan(0)
            .When(x => x.EstimatedLossAmount.HasValue)
            .WithMessage("Estimated loss amount must be greater than zero.");

        RuleFor(x => x.InitialReserveAmount)
            .GreaterThan(0)
            .When(x => x.InitialReserveAmount.HasValue)
            .WithMessage("Initial reserve amount must be greater than zero.");

        RuleFor(x => x.InitialReserveComponent)
            .NotNull()
            .When(x => x.InitialReserveAmount.HasValue)
            .WithMessage("Reserve component is required when initial reserve amount is provided.");
    }
}