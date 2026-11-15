using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

internal abstract class MasterConfiguration<T>(string table) : IEntityTypeConfiguration<T> where T : MasterEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(table);
        builder.ConfigureBase();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Code).HasMaxLength(20);
        builder.Property(e => e.Description).HasMaxLength(500);
    }

    protected static void UniqueName(EntityTypeBuilder<T> builder) =>
        builder.HasIndex(e => e.Name).IsUnique().HasFilter(BaseEntityConfiguration.ActiveFilter);
}

internal sealed class DepartmentConfiguration() : MasterConfiguration<Department>("Departments")
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        base.Configure(builder);
        UniqueName(builder);
    }
}

internal sealed class DesignationConfiguration() : MasterConfiguration<Designation>("Designations")
{
    public override void Configure(EntityTypeBuilder<Designation> builder)
    {
        base.Configure(builder);
        UniqueName(builder);
    }
}

internal sealed class EmployeeTypeConfiguration() : MasterConfiguration<EmployeeType>("EmployeeTypes")
{
    public override void Configure(EntityTypeBuilder<EmployeeType> builder)
    {
        base.Configure(builder);
        UniqueName(builder);
    }
}

internal sealed class RelationshipConfiguration() : MasterConfiguration<Relationship>("Relationships")
{
    public override void Configure(EntityTypeBuilder<Relationship> builder)
    {
        base.Configure(builder);
        UniqueName(builder);
    }
}

internal sealed class CountryConfiguration() : MasterConfiguration<Country>("Countries")
{
    public override void Configure(EntityTypeBuilder<Country> builder)
    {
        base.Configure(builder);
        UniqueName(builder);
    }
}

internal sealed class StateConfiguration() : MasterConfiguration<State>("States")
{
    public override void Configure(EntityTypeBuilder<State> builder)
    {
        base.Configure(builder);

        builder.HasOne(s => s.Country)
            .WithMany(c => c.States)
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.CountryId, s.Name }).IsUnique().HasFilter(BaseEntityConfiguration.ActiveFilter);
    }
}

internal sealed class CityConfiguration() : MasterConfiguration<City>("Cities")
{
    public override void Configure(EntityTypeBuilder<City> builder)
    {
        base.Configure(builder);

        builder.HasOne(c => c.State)
            .WithMany(s => s.Cities)
            .HasForeignKey(c => c.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.StateId, c.Name }).IsUnique().HasFilter(BaseEntityConfiguration.ActiveFilter);
    }
}
