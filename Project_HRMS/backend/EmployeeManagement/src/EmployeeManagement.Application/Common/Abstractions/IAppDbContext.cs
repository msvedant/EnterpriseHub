using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EmployeeManagement.Application.Common.Abstractions;

/// <summary>Persistence abstraction used by handlers. Implemented by the EF Core DbContext.</summary>
public interface IAppDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Designation> Designations { get; }
    DbSet<EmployeeType> EmployeeTypes { get; }
    DbSet<Relationship> Relationships { get; }
    DbSet<Country> Countries { get; }
    DbSet<State> States { get; }
    DbSet<City> Cities { get; }
    DbSet<Employee> Employees { get; }
    DbSet<EmployeeFamily> EmployeeFamilies { get; }
    DbSet<EmployeeAddress> EmployeeAddresses { get; }
    DbSet<EmployeeEducation> EmployeeEducations { get; }
    DbSet<EmployeeExperience> EmployeeExperiences { get; }
    DbSet<User> Users { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="action"/> inside a database transaction (wrapped in the provider's
    /// execution strategy so transient-failure retries re-run the whole unit of work).
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
