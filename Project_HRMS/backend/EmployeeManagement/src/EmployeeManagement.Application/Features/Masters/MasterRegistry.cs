using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Features.Masters;

/// <summary>Maps each master kind / URL slug to its accessor and referential-integrity rules.</summary>
public static class MasterRegistry
{
    private static readonly Dictionary<MasterKind, IMasterAccessor> Accessors = new()
    {
        [MasterKind.Department] = new MasterAccessor<Department>("Department",
            inUse: (db, id, ct) => db.Employees.AnyAsync(e => e.DepartmentId == id, ct)),

        [MasterKind.Designation] = new MasterAccessor<Designation>("Designation",
            inUse: (db, id, ct) => db.Employees.AnyAsync(e => e.DesignationId == id, ct)),

        [MasterKind.EmployeeType] = new MasterAccessor<EmployeeType>("Employee type",
            inUse: (db, id, ct) => db.Employees.AnyAsync(e => e.EmployeeTypeId == id, ct)),

        [MasterKind.Relationship] = new MasterAccessor<Relationship>("Relationship",
            inUse: (db, id, ct) => db.EmployeeFamilies.AnyAsync(f => f.RelationshipId == id, ct)),

        [MasterKind.Country] = new MasterAccessor<Country>("Country",
            inUse: async (db, id, ct) =>
                await db.States.AnyAsync(s => s.CountryId == id, ct) ||
                await db.EmployeeAddresses.AnyAsync(a => a.CountryId == id, ct)),

        [MasterKind.State] = new MasterAccessor<State>("State",
            parentProperty: nameof(State.CountryId),
            parentExists: (db, id, ct) => db.Countries.AnyAsync(c => c.Id == id, ct),
            inUse: async (db, id, ct) =>
                await db.Cities.AnyAsync(c => c.StateId == id, ct) ||
                await db.EmployeeAddresses.AnyAsync(a => a.StateId == id, ct)),

        [MasterKind.City] = new MasterAccessor<City>("City",
            parentProperty: nameof(City.StateId),
            parentExists: (db, id, ct) => db.States.AnyAsync(s => s.Id == id, ct),
            inUse: (db, id, ct) => db.EmployeeAddresses.AnyAsync(a => a.CityId == id, ct))
    };

    private static readonly Dictionary<string, MasterKind> Slugs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["departments"] = MasterKind.Department,
        ["designations"] = MasterKind.Designation,
        ["employee-types"] = MasterKind.EmployeeType,
        ["relationships"] = MasterKind.Relationship,
        ["countries"] = MasterKind.Country,
        ["states"] = MasterKind.State,
        ["cities"] = MasterKind.City
    };

    public static IMasterAccessor Get(MasterKind kind) => Accessors[kind];

    public static bool TryParseSlug(string slug, out MasterKind kind) => Slugs.TryGetValue(slug, out kind);

}
