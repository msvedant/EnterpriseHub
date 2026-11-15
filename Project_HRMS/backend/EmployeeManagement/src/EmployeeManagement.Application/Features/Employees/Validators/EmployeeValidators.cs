using EmployeeManagement.Application.Features.Employees.Commands;
using EmployeeManagement.Application.Features.Employees.Queries;
using FluentValidation;

namespace EmployeeManagement.Application.Features.Employees.Validators;

public sealed class FamilyInputValidator : AbstractValidator<FamilyInput>
{
    public FamilyInputValidator(TimeProvider time)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.RelationshipId).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DateOfBirth)
            .Must(d => d!.Value <= DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime))
            .WithMessage("Date of birth cannot be in the future.")
            .When(x => x.DateOfBirth.HasValue);
        RuleFor(x => x.Occupation).MaximumLength(100);
        RuleFor(x => x.Phone).Matches(@"^\+?[0-9]{7,15}$").WithMessage("Phone must contain 7 to 15 digits and may start with '+'.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

public sealed class AddressInputValidator : AbstractValidator<AddressInput>
{
    public AddressInputValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.AddressType).IsInEnum();
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Line2).MaximumLength(200);
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.StateId).GreaterThan(0);
        RuleFor(x => x.CityId).GreaterThan(0);
        RuleFor(x => x.PostalCode).NotEmpty().Matches(@"^[A-Za-z0-9\- ]{3,12}$")
            .WithMessage("Postal code must be 3 to 12 letters, digits, spaces or hyphens.");
    }
}

public sealed class EducationInputValidator : AbstractValidator<EducationInput>
{
    public EducationInputValidator(TimeProvider time)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FieldOfStudy).MaximumLength(150);
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);
        RuleFor(x => x.YearOfPassing)
            .Must(y => y >= 1950 && y <= time.GetUtcNow().Year + 6)
            .WithMessage("Year of passing is out of range.");
        RuleFor(x => x.Percentage).InclusiveBetween(0m, 100m).When(x => x.Percentage.HasValue);
    }
}

public sealed class ExperienceInputValidator : AbstractValidator<ExperienceInput>
{
    public ExperienceInputValidator(TimeProvider time)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        DateOnly Today() => DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.StartDate)
            .Must(d => d != default).WithMessage("Start date is required.")
            .Must(d => d <= Today()).WithMessage("Start date cannot be in the future.");
        RuleFor(x => x.EndDate)
            .Must((x, end) => end!.Value >= x.StartDate).WithMessage("End date cannot be before the start date.")
            .Must(end => end!.Value <= Today()).WithMessage("End date cannot be in the future.")
            .When(x => x.EndDate.HasValue);
        RuleFor(x => x.Responsibilities).MaximumLength(1000);
        RuleFor(x => x.ReasonForLeaving).MaximumLength(500);
    }
}

public sealed class EmployeeRequestValidator : AbstractValidator<EmployeeRequest>
{
    private const int MaxDetailRows = 20;

    public EmployeeRequestValidator(TimeProvider time)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        DateOnly Today() => DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Employee code may contain only letters, digits, '-' and '_'.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .Must(d => d != default).WithMessage("Date of birth is required.")
            .Must(d => d <= Today().AddYears(-16)).WithMessage("Employee must be at least 16 years old.")
            .Must(d => d >= Today().AddYears(-100)).WithMessage("Date of birth is out of range.");

        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Mobile).NotEmpty().Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("Mobile must contain 7 to 15 digits and may start with '+'.");

        RuleFor(x => x.JoiningDate)
            .Must(d => d != default).WithMessage("Joining date is required.")
            .Must(d => d <= Today().AddYears(1)).WithMessage("Joining date cannot be more than a year in the future.")
            .Must((x, d) => x.DateOfBirth.Year > 9000 || d >= x.DateOfBirth.AddYears(16))
            .WithMessage("Joining date must be at least 16 years after the date of birth.");

        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.DesignationId).GreaterThan(0);
        RuleFor(x => x.EmployeeTypeId).GreaterThan(0);
        RuleFor(x => x.ReportingManagerId).GreaterThan(0).When(x => x.ReportingManagerId.HasValue);

        RuleFor(x => x.Families).NotNull().Must(l => l.Count <= MaxDetailRows)
            .WithMessage($"At most {MaxDetailRows} family members are allowed.");
        RuleForEach(x => x.Families).SetValidator(new FamilyInputValidator(time));

        RuleFor(x => x.Addresses).NotNull().Must(l => l.Count <= MaxDetailRows)
            .WithMessage($"At most {MaxDetailRows} addresses are allowed.")
            .Must(l => l.Count(a => a.IsPrimary) <= 1).WithMessage("Only one address can be marked as primary.");
        RuleForEach(x => x.Addresses).SetValidator(new AddressInputValidator());

        RuleFor(x => x.Educations).NotNull().Must(l => l.Count <= MaxDetailRows)
            .WithMessage($"At most {MaxDetailRows} education records are allowed.");
        RuleForEach(x => x.Educations).SetValidator(new EducationInputValidator(time));

        RuleFor(x => x.Experiences).NotNull().Must(l => l.Count <= MaxDetailRows)
            .WithMessage($"At most {MaxDetailRows} experience records are allowed.");
        RuleForEach(x => x.Experiences).SetValidator(new ExperienceInputValidator(time));
    }
}

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator(IValidator<EmployeeRequest> inner)
    {
        RuleFor(x => x.Data).NotNull().SetValidator(inner);
    }
}

public sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator(IValidator<EmployeeRequest> inner)
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data).NotNull().SetValidator(inner);
        RuleFor(x => x.Data.RowVersion).NotNull().WithMessage("RowVersion is required.")
            .When(x => x.Data is not null);
    }
}

public sealed class GetEmployeesQueryValidator : AbstractValidator<GetEmployeesQuery>
{
    public GetEmployeesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.SortBy)
            .Must(s => GetEmployeesQueryHandler.SortColumns.ContainsKey(s))
            .WithMessage($"SortBy must be one of: {string.Join(", ", GetEmployeesQueryHandler.SortColumns.Keys)}.");
        RuleFor(x => x.SortDirection)
            .Must(s => s.Equals("asc", StringComparison.OrdinalIgnoreCase) || s.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
        RuleFor(x => x.Gender).IsInEnum().When(x => x.Gender.HasValue);
    }
}
