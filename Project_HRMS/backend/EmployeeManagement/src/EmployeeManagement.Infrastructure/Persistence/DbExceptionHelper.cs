using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

public static class DbExceptionHelper
{
    public static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };

    public static bool IsForeignKeyViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 547 };
}
