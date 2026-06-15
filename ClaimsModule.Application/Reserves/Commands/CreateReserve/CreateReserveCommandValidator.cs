using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enumerations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Application.Reserves.Commands.CreateReserve;

public class CreateReserveCommandValidator : AbstractValidator<CreateReserveCommand>
{
    public CreateReserveCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .MustAsync(async (id, ct) =>
                await context.Claims.AnyAsync(c => c.Id == id && !c.IsDeleted, ct))
            .WithMessage("Claim not found.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Reserve amount must be greater than zero.")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Reserve amount cannot exceed $10,000,000 aggregate cap.");

        RuleFor(x => x.ChangeReason)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(1000)
            .WithMessage("Change reason must be between 5 and 1000 characters.");

        RuleFor(x => x.Component)
            .IsInEnum()
            .WithMessage("Invalid reserve component.");
    }
}