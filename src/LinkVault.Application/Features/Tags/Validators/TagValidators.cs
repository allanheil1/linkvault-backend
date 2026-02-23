using FluentValidation;
using LinkVault.Application.Features.Tags.Commands;

namespace LinkVault.Application.Features.Tags.Validators;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
    }
}

public class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
    }
}
