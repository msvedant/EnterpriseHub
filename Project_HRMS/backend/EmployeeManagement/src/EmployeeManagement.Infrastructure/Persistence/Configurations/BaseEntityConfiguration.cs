using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

internal static class BaseEntityConfiguration
{
    public const string ActiveFilter = "[IsDeleted] = 0";

    public static EntityTypeBuilder<T> ConfigureBase<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.DeletedBy).HasMaxLength(256);
        builder.Property(e => e.RowVersion).IsRowVersion();
        return builder;
    }
}
