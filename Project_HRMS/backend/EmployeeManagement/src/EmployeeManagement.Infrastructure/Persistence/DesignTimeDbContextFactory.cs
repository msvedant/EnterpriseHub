using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeManagement.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` create the context without starting the web host.
/// Override the connection with the EMS_CONNECTION environment variable.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("EMS_CONNECTION")
            ?? "Server=localhost,1433;Database=EmployeeManagementDb;User Id=sa;Password=Ems_Dev#2025x;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new AppDbContext(options);
    }
}
