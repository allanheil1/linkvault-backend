using FluentValidation;
using LinkVault.Application.Features.Links.Commands;
using LinkVault.Application.Features.Links.Queries;

namespace LinkVault.Application.Features.Links.Validators;

public class CreateLinkCommandValidator : AbstractValidator<CreateLinkCommand>
{
    public CreateLinkCommandValidator()
    {
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class UpdateLinkCommandValidator : AbstractValidator<UpdateLinkCommand>
{
    public UpdateLinkCommandValidator()
    {
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class ListLinksQueryValidator : AbstractValidator<ListLinksQuery>
{
    public ListLinksQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
