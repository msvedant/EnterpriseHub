using EmployeeManagement.Application.Features.Employees;
using EmployeeManagement.Application.Features.Employees.Validators;
using EmployeeManagement.UnitTests.Helpers;

namespace EmployeeManagement.UnitTests.Unit;

public class EmployeeRequestValidatorTests
{
    private static readonly LookupIds Ids = new(1, 1, 1, 1, 1, 1, 1);
    private readonly EmployeeRequestValidator _validator = new(TimeProvider.System);

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.Validate(TestDb.ValidRequest(Ids));
        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void Future_date_of_birth_fails()
    {
        var request = TestDb.ValidRequest(Ids) with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) };
        var result = _validator.Validate(request);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EmployeeRequest.DateOfBirth));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Invalid_email_fails(string email)
    {
        var result = _validator.Validate(TestDb.ValidRequest(Ids) with { Email = email });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EmployeeRequest.Email));
    }

    [Fact]
    public void Invalid_mobile_fails()
    {
        var result = _validator.Validate(TestDb.ValidRequest(Ids) with { Mobile = "12ab" });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EmployeeRequest.Mobile));
    }

    [Fact]
    public void More_than_one_primary_address_fails()
    {
        var request = TestDb.ValidRequest(Ids);
        var duplicate = request.Addresses[0] with { };
        var result = _validator.Validate(request with { Addresses = [request.Addresses[0], duplicate] });
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("primary"));
    }

    [Fact]
    public void Experience_end_before_start_fails()
    {
        var request = TestDb.ValidRequest(Ids);
        var bad = request.Experiences[0] with { StartDate = new DateOnly(2020, 1, 1), EndDate = new DateOnly(2019, 1, 1) };
        var result = _validator.Validate(request with { Experiences = [bad] });
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("End date"));
    }

    [Fact]
    public void Percentage_above_100_fails()
    {
        var request = TestDb.ValidRequest(Ids);
        var bad = request.Educations[0] with { Percentage = 120m };
        var result = _validator.Validate(request with { Educations = [bad] });
        Assert.False(result.IsValid);
    }
}
