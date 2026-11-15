using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService? currentUser = null,
    TimeProvider? timeProvider = null) : DbContext(options), IAppDbContext
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Designation> Designations { get; set; } = null!;
    public DbSet<EmployeeType> EmployeeTypes { get; set; } = null!;
    public DbSet<Relationship> Relationships { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<State> States { get; set; } = null!;
    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<EmployeeFamily> EmployeeFamilies { get; set; } = null!;
    public DbSet<EmployeeAddress> EmployeeAddresses { get; set; } = null!;
    public DbSet<EmployeeEducation> EmployeeEducations { get; set; } = null!;
    public DbSet<EmployeeExperience> EmployeeExperiences { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Soft-delete filter on every entity (children included, so filtered parents never leave orphaned required navigations).
        ApplySoftDeleteFilter<Department>(modelBuilder);
        ApplySoftDeleteFilter<Designation>(modelBuilder);
        ApplySoftDeleteFilter<EmployeeType>(modelBuilder);
        ApplySoftDeleteFilter<Relationship>(modelBuilder);
        ApplySoftDeleteFilter<Country>(modelBuilder);
        ApplySoftDeleteFilter<State>(modelBuilder);
        ApplySoftDeleteFilter<City>(modelBuilder);
        ApplySoftDeleteFilter<Employee>(modelBuilder);
        ApplySoftDeleteFilter<EmployeeFamily>(modelBuilder);
        ApplySoftDeleteFilter<EmployeeAddress>(modelBuilder);
        ApplySoftDeleteFilter<EmployeeEducation>(modelBuilder);
        ApplySoftDeleteFilter<EmployeeExperience>(modelBuilder);
        ApplySoftDeleteFilter<User>(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity =>
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        // Non-relational providers (in-memory tests) have no transactions.
        if (!Database.IsRelational())
            return await action(cancellationToken);

        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            // A retry re-runs the delegate, so start from a clean change tracker.
            ChangeTracker.Clear();
            await using var transaction = await Database.BeginTransactionAsync(ct);
            var result = await action(ct);
            await transaction.CommitAsync(ct);
            return result;
        }, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var user = currentUser?.UserName ?? "system";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt is null)
                    {
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = user;
                    }
                    break;
            }
        }
    }
}
