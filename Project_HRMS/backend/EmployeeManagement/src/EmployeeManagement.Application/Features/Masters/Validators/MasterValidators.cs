using EmployeeManagement.Application.Features.Masters.Commands;
using EmployeeManagement.Application.Features.Masters.Queries;
using FluentValidation;

namespace EmployeeManagement.Application.Features.Masters.Validators;

public sealed class MasterRequestValidator : AbstractValidator<MasterRequest>
{
    public MasterRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
    }
}

public sealed class CreateMasterCommandValidator : AbstractValidator<CreateMasterCommand>
{
    public CreateMasterCommandValidator(IValidator<MasterRequest> inner)
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Data).NotNull().SetValidator(inner);
    }
}

public sealed class UpdateMasterCommandValidator : AbstractValidator<UpdateMasterCommand>
{
    public UpdateMasterCommandValidator(IValidator<MasterRequest> inner)
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data).NotNull().SetValidator(inner);
        RuleFor(x => x.Data.RowVersion).NotNull().WithMessage("RowVersion is required.")
            .When(x => x.Data is not null);
    }
}

public sealed class GetMastersQueryValidator : AbstractValidator<GetMastersQuery>
{
    public GetMastersQueryValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Filter.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.Filter.Search).MaximumLength(100);
    }
}
