using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Infrastructure.Persistence;

/// <summary>Applies migrations (optional) and seeds master data and the first admin account idempotently.</summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var config = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var db = provider.GetRequiredService<AppDbContext>();

        if (config.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        {
            if (db.Database.IsRelational() && db.Database.GetMigrations().Any())
            {
                logger.LogInformation("Applying database migrations");
                await db.Database.MigrateAsync(ct);
            }
            else
            {
                // No migrations generated yet (or non-relational provider): create the schema directly.
                logger.LogWarning("No EF migrations found; creating the schema with EnsureCreated (development only)");
                await db.Database.EnsureCreatedAsync(ct);
            }
        }

        if (config.GetValue("Database:SeedOnStartup", true))
        {
            await SeedMasterDataAsync(db, ct);
            await SeedAdminAsync(db, config, provider.GetRequiredService<IPasswordService>(), logger, ct);
        }
    }

    private static async Task SeedMasterDataAsync(AppDbContext db, CancellationToken ct)
    {
        if (!await db.Departments.AnyAsync(ct))
        {
            db.Departments.AddRange(
                New<Department>("Human Resources", "HR"), New<Department>("Engineering", "ENG"),
                New<Department>("Finance", "FIN"), New<Department>("Sales", "SAL"),
                New<Department>("Marketing", "MKT"), New<Department>("Operations", "OPS"),
                New<Department>("Information Technology", "IT"), New<Department>("Legal", "LEG"));
        }

        if (!await db.Designations.AnyAsync(ct))
        {
            db.Designations.AddRange(
                New<Designation>("Chief Executive Officer", "CEO"), New<Designation>("Director", "DIR"),
                New<Designation>("Senior Manager", "SMGR"), New<Designation>("Manager", "MGR"),
                New<Designation>("Team Lead", "TL"), New<Designation>("Senior Software Engineer", "SSE"),
                New<Designation>("Software Engineer", "SE"), New<Designation>("HR Executive", "HRE"),
                New<Designation>("Accountant", "ACC"), New<Designation>("Sales Executive", "SEX"));
        }

        if (!await db.EmployeeTypes.AnyAsync(ct))
        {
            db.EmployeeTypes.AddRange(
                New<EmployeeType>("Full-Time", "FT"), New<EmployeeType>("Part-Time", "PT"),
                New<EmployeeType>("Contract", "CON"), New<EmployeeType>("Intern", "INT"),
                New<EmployeeType>("Consultant", "CNS"));
        }

        if (!await db.Relationships.AnyAsync(ct))
        {
            db.Relationships.AddRange(
                New<Relationship>("Father"), New<Relationship>("Mother"), New<Relationship>("Spouse"),
                New<Relationship>("Son"), New<Relationship>("Daughter"), New<Relationship>("Brother"),
                New<Relationship>("Sister"), New<Relationship>("Guardian"), New<Relationship>("Other"));
        }

        if (!await db.Countries.AnyAsync(ct))
        {
            var india = New<Country>("India", "IN");
            var usa = New<Country>("United States", "US");
            var uk = New<Country>("United Kingdom", "GB");
            db.Countries.AddRange(india, usa, uk,
                New<Country>("Canada", "CA"), New<Country>("Australia", "AU"),
                New<Country>("United Arab Emirates", "AE"), New<Country>("Singapore", "SG"),
                New<Country>("Germany", "DE"));

            var maharashtra = Child(new State { Country = india }, "Maharashtra", "MH");
            var karnataka = Child(new State { Country = india }, "Karnataka", "KA");
            var delhi = Child(new State { Country = india }, "Delhi", "DL");
            var tamilNadu = Child(new State { Country = india }, "Tamil Nadu", "TN");
            var gujarat = Child(new State { Country = india }, "Gujarat", "GJ");
            var california = Child(new State { Country = usa }, "California", "CA");
            var texas = Child(new State { Country = usa }, "Texas", "TX");
            var newYork = Child(new State { Country = usa }, "New York", "NY");
            var england = Child(new State { Country = uk }, "England", "ENG");
            db.States.AddRange(maharashtra, karnataka, delhi, tamilNadu, gujarat, california, texas, newYork, england);

            db.Cities.AddRange(
                Child(new City { State = maharashtra }, "Mumbai"), Child(new City { State = maharashtra }, "Pune"),
                Child(new City { State = maharashtra }, "Nagpur"), Child(new City { State = maharashtra }, "Navi Mumbai"),
                Child(new City { State = karnataka }, "Bengaluru"), Child(new City { State = karnataka }, "Mysuru"),
                Child(new City { State = delhi }, "New Delhi"), Child(new City { State = tamilNadu }, "Chennai"),
                Child(new City { State = gujarat }, "Ahmedabad"),
                Child(new City { State = california }, "Los Angeles"), Child(new City { State = california }, "San Francisco"),
                Child(new City { State = texas }, "Houston"), Child(new City { State = texas }, "Austin"),
                Child(new City { State = newYork }, "New York City"), Child(new City { State = england }, "London"));
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAdminAsync(AppDbContext db, IConfiguration config, IPasswordService passwords,
        ILogger logger, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin, ct))
            return;

        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No admin user exists and Seed:AdminEmail / Seed:AdminPassword are not configured; skipping admin seed");
            return;
        }

        db.Users.Add(new User
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = config["Seed:AdminFullName"] ?? "System Administrator",
            Role = UserRole.Admin,
            PasswordHash = passwords.Hash(password)
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded initial administrator account");
    }

    private static T New<T>(string name, string? code = null) where T : MasterEntity, new() =>
        new() { Name = name, Code = code };

    private static T Child<T>(T entity, string name, string? code = null) where T : MasterEntity
    {
        entity.Name = name;
        entity.Code = code;
        return entity;
    }
}
