using FluentValidation;
using LinkVault.Application.Features.Collections.Commands;
using LinkVault.Application.Features.Collections.Queries;

namespace LinkVault.Application.Features.Collections.Validators;

public class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
{
    public UpdateCollectionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class ListCollectionLinksQueryValidator : AbstractValidator<ListCollectionLinksQuery>
{
    public ListCollectionLinksQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
