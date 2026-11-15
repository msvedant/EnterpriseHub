using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Application.Features.Employees;
using EmployeeManagement.Application.Features.Employees.Commands;
using EmployeeManagement.Application.Features.Employees.Queries;
using EmployeeManagement.UnitTests.Helpers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.UnitTests.Unit;

public class EmployeeHandlerTests
{
    [Fact]
    public async Task Create_persists_header_and_all_details()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);

        var dto = await new CreateEmployeeCommandHandler(db)
            .Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        Assert.True(dto.Id > 0);
        Assert.Equal("EMP001", dto.EmployeeCode);
        Assert.Equal("Engineering", dto.Department);
        Assert.Single(dto.Families);
        Assert.Equal("Spouse", dto.Families[0].Relationship);
        Assert.Single(dto.Addresses);
        Assert.Equal("Pune", dto.Addresses[0].City);
        Assert.Single(dto.Educations);
        Assert.Single(dto.Experiences);
        Assert.Equal("tester@example.com", dto.CreatedBy);
    }

    [Fact]
    public async Task Create_rejects_duplicate_employee_code()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var handler = new CreateEmployeeCommandHandler(db);
        await handler.Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new CreateEmployeeCommand(TestDb.ValidRequest(ids, email: "other@example.com")), CancellationToken.None));
    }

    [Fact]
    public async Task Create_rejects_unknown_department()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var request = TestDb.ValidRequest(ids) with { DepartmentId = 9999 };

        await Assert.ThrowsAsync<ValidationException>(() =>
            new CreateEmployeeCommandHandler(db).Handle(new CreateEmployeeCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Create_rejects_city_that_does_not_belong_to_state()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var request = TestDb.ValidRequest(ids);
        var bad = request.Addresses[0] with { CityId = 9999 };

        await Assert.ThrowsAsync<ValidationException>(() => new CreateEmployeeCommandHandler(db)
            .Handle(new CreateEmployeeCommand(request with { Addresses = [bad] }), CancellationToken.None));
    }

    [Fact]
    public async Task Update_adds_updates_and_removes_detail_records()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var created = await new CreateEmployeeCommandHandler(db)
            .Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        var request = TestDb.ValidRequest(ids) with
        {
            FirstName = "Jonathan",
            RowVersion = created.RowVersion,
            // update existing family member
            Families = [new FamilyInput { Id = created.Families[0].Id, RelationshipId = ids.RelationshipId, FullName = "Jane A. Doe" }],
            // keep address unchanged
            Addresses = [new AddressInput
            {
                Id = created.Addresses[0].Id, AddressType = created.Addresses[0].AddressType, Line1 = "99 New Road",
                CountryId = ids.CountryId, StateId = ids.StateId, CityId = ids.CityId, PostalCode = "411001", IsPrimary = true
            }],
            // add a new education record, keep existing
            Educations =
            [
                new EducationInput { Id = created.Educations[0].Id, Degree = "B.Tech", Institution = "State University", YearOfPassing = 2012 },
                new EducationInput { Degree = "M.Tech", Institution = "Tech Institute", YearOfPassing = 2014 }
            ],
            // remove all experience
            Experiences = []
        };

        var updated = await new UpdateEmployeeCommandHandler(db)
            .Handle(new UpdateEmployeeCommand(created.Id, request), CancellationToken.None);

        Assert.Equal("Jonathan", updated.FirstName);
        Assert.Equal("Jane A. Doe", Assert.Single(updated.Families).FullName);
        Assert.Equal("99 New Road", Assert.Single(updated.Addresses).Line1);
        Assert.Equal(2, updated.Educations.Count);
        Assert.Empty(updated.Experiences);
    }

    [Fact]
    public async Task Update_rejects_detail_id_from_another_employee()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var created = await new CreateEmployeeCommandHandler(db)
            .Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        var request = TestDb.ValidRequest(ids) with
        {
            RowVersion = created.RowVersion,
            Families = [new FamilyInput { Id = 9999, RelationshipId = ids.RelationshipId, FullName = "Ghost" }]
        };

        await Assert.ThrowsAsync<ValidationException>(() => new UpdateEmployeeCommandHandler(db)
            .Handle(new UpdateEmployeeCommand(created.Id, request), CancellationToken.None));
    }

    [Fact]
    public async Task Update_unknown_employee_throws_not_found()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);

        await Assert.ThrowsAsync<NotFoundException>(() => new UpdateEmployeeCommandHandler(db).Handle(
            new UpdateEmployeeCommand(12345, TestDb.ValidRequest(ids) with { RowVersion = "" }), CancellationToken.None));
    }

    [Fact]
    public async Task Update_rejects_self_as_reporting_manager()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var created = await new CreateEmployeeCommandHandler(db)
            .Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        var request = TestDb.ValidRequest(ids) with { RowVersion = created.RowVersion, ReportingManagerId = created.Id };

        await Assert.ThrowsAsync<ValidationException>(() => new UpdateEmployeeCommandHandler(db)
            .Handle(new UpdateEmployeeCommand(created.Id, request), CancellationToken.None));
    }

    [Fact]
    public async Task Soft_delete_hides_employee_and_frees_the_code()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var create = new CreateEmployeeCommandHandler(db);
        var created = await create.Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        await new DeleteEmployeeCommandHandler(db).Handle(new DeleteEmployeeCommand(created.Id), CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetEmployeeByIdQueryHandler(db).Handle(new GetEmployeeByIdQuery(created.Id), CancellationToken.None));

        // The row is retained for audit purposes.
        Assert.Single(db.Employees.IgnoreQueryFilters().Where(e => e.Id == created.Id && e.IsDeleted));

        var again = await create.Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);
        Assert.NotEqual(created.Id, again.Id);
    }

    [Fact]
    public async Task Deactivate_and_activate_toggle_status()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var created = await new CreateEmployeeCommandHandler(db)
            .Handle(new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);
        var handler = new SetEmployeeStatusCommandHandler(db);

        await handler.Handle(new SetEmployeeStatusCommand(created.Id, false), CancellationToken.None);
        var inactive = await new GetEmployeeByIdQueryHandler(db).Handle(new GetEmployeeByIdQuery(created.Id), CancellationToken.None);
        Assert.False(inactive.IsActive);

        await handler.Handle(new SetEmployeeStatusCommand(created.Id, true), CancellationToken.None);
        var active = await new GetEmployeeByIdQueryHandler(db).Handle(new GetEmployeeByIdQuery(created.Id), CancellationToken.None);
        Assert.True(active.IsActive);
    }

    [Fact]
    public async Task List_supports_paging_sorting_and_filtering()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var create = new CreateEmployeeCommandHandler(db);
        foreach (var (code, first) in new[] { ("E001", "Alice"), ("E002", "Bob"), ("E003", "Carol") })
        {
            var request = TestDb.ValidRequest(ids, code, $"{first.ToLowerInvariant()}@example.com") with { FirstName = first };
            await create.Handle(new CreateEmployeeCommand(request), CancellationToken.None);
        }

        var handler = new GetEmployeesQueryHandler(db);

        var page = await handler.Handle(new GetEmployeesQuery { Page = 1, PageSize = 2, SortBy = "firstName", SortDirection = "desc" },
            CancellationToken.None);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.Equal(new[] { "Carol Doe", "Bob Doe" }, page.Items.Select(i => i.FullName).ToArray());

        var filtered = await handler.Handle(new GetEmployeesQuery { Search = "alice" }, CancellationToken.None);
        Assert.Equal("E001", Assert.Single(filtered.Items).EmployeeCode);
    }
}
