using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Application.Features.Employees.Commands;
using EmployeeManagement.Application.Features.Masters;
using EmployeeManagement.Application.Features.Masters.Commands;
using EmployeeManagement.Application.Features.Masters.Queries;
using EmployeeManagement.UnitTests.Helpers;
using FluentValidation;

namespace EmployeeManagement.UnitTests.Unit;

public class MasterHandlerTests
{
    [Fact]
    public async Task Create_then_get_department()
    {
        await using var db = TestDb.Create();

        var created = await new CreateMasterCommandHandler(db).Handle(
            new CreateMasterCommand(MasterKind.Department, new MasterRequest { Name = " Finance ", Code = "fin" }),
            CancellationToken.None);

        Assert.Equal("Finance", created.Name);
        Assert.Equal("FIN", created.Code);

        var fetched = await new GetMasterByIdQueryHandler(db).Handle(
            new GetMasterByIdQuery(MasterKind.Department, created.Id), CancellationToken.None);
        Assert.Equal(created.Id, fetched.Id);
    }

    [Fact]
    public async Task Duplicate_name_is_rejected()
    {
        await using var db = TestDb.Create();
        var handler = new CreateMasterCommandHandler(db);
        var command = new CreateMasterCommand(MasterKind.Designation, new MasterRequest { Name = "Manager" });
        await handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task State_requires_an_existing_country()
    {
        await using var db = TestDb.Create();
        var handler = new CreateMasterCommandHandler(db);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(
            new CreateMasterCommand(MasterKind.State, new MasterRequest { Name = "Nowhere" }), CancellationToken.None));
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(
            new CreateMasterCommand(MasterKind.State, new MasterRequest { Name = "Nowhere", ParentId = 777 }), CancellationToken.None));
    }

    [Fact]
    public async Task Same_city_name_is_allowed_under_different_states()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        var create = new CreateMasterCommandHandler(db);

        var otherState = await create.Handle(new CreateMasterCommand(MasterKind.State,
            new MasterRequest { Name = "Karnataka", ParentId = ids.CountryId }), CancellationToken.None);

        var city = await create.Handle(new CreateMasterCommand(MasterKind.City,
            new MasterRequest { Name = "Pune", ParentId = otherState.Id }), CancellationToken.None);

        Assert.Equal(otherState.Id, city.ParentId);
    }

    [Fact]
    public async Task Update_changes_values()
    {
        await using var db = TestDb.Create();
        var created = await new CreateMasterCommandHandler(db).Handle(
            new CreateMasterCommand(MasterKind.EmployeeType, new MasterRequest { Name = "Contract" }), CancellationToken.None);

        var updated = await new UpdateMasterCommandHandler(db).Handle(new UpdateMasterCommand(MasterKind.EmployeeType, created.Id,
            new MasterRequest { Name = "Contractor", RowVersion = created.RowVersion }), CancellationToken.None);

        Assert.Equal("Contractor", updated.Name);
    }

    [Fact]
    public async Task Delete_is_blocked_while_the_master_is_in_use()
    {
        await using var db = TestDb.Create();
        var ids = await TestDb.SeedLookupsAsync(db);
        await new CreateEmployeeCommandHandler(db).Handle(
            new CreateEmployeeCommand(TestDb.ValidRequest(ids)), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => new DeleteMasterCommandHandler(db).Handle(
            new DeleteMasterCommand(MasterKind.Department, ids.DepartmentId), CancellationToken.None));
    }

    [Fact]
    public async Task Unused_master_is_soft_deleted_and_hidden()
    {
        await using var db = TestDb.Create();
        var created = await new CreateMasterCommandHandler(db).Handle(
            new CreateMasterCommand(MasterKind.Relationship, new MasterRequest { Name = "Cousin" }), CancellationToken.None);

        await new DeleteMasterCommandHandler(db).Handle(new DeleteMasterCommand(MasterKind.Relationship, created.Id), CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => new GetMasterByIdQueryHandler(db).Handle(
            new GetMasterByIdQuery(MasterKind.Relationship, created.Id), CancellationToken.None));
    }

    [Fact]
    public async Task List_filters_by_search_and_status()
    {
        await using var db = TestDb.Create();
        var create = new CreateMasterCommandHandler(db);
        var a = await create.Handle(new CreateMasterCommand(MasterKind.Department, new MasterRequest { Name = "Alpha" }), CancellationToken.None);
        await create.Handle(new CreateMasterCommand(MasterKind.Department, new MasterRequest { Name = "Beta" }), CancellationToken.None);
        await new SetMasterStatusCommandHandler(db).Handle(new SetMasterStatusCommand(MasterKind.Department, a.Id, false), CancellationToken.None);

        var result = await new GetMastersQueryHandler(db).Handle(
            new GetMastersQuery(MasterKind.Department, new MasterListFilter { IsActive = true }), CancellationToken.None);

        Assert.Equal("Beta", Assert.Single(result.Items).Name);
    }

    [Theory]
    [InlineData("departments", MasterKind.Department)]
    [InlineData("EMPLOYEE-TYPES", MasterKind.EmployeeType)]
    [InlineData("cities", MasterKind.City)]
    public void Slugs_resolve_case_insensitively(string slug, MasterKind expected)
    {
        Assert.True(MasterRegistry.TryParseSlug(slug, out var kind));
        Assert.Equal(expected, kind);
    }

    [Fact]
    public void Unknown_slug_is_not_resolved() => Assert.False(MasterRegistry.TryParseSlug("payroll", out _));
}
