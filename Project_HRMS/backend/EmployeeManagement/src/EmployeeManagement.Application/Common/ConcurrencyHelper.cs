using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Common;

internal static class ConcurrencyHelper
{
    /// <summary>Applies the client supplied row version as the original value so EF enforces optimistic concurrency.</summary>
    public static void ApplyRowVersion<TEntity>(IAppDbContext db, TEntity entity, string? rowVersion)
        where TEntity : BaseEntity
    {
        if (rowVersion is null)
            throw Fail.Validation("rowVersion", "RowVersion is required.");

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            throw Fail.Validation("rowVersion", "RowVersion is not valid.");
        }

        db.Entry(entity).Property(e => e.RowVersion).OriginalValue = bytes;
    }
}
