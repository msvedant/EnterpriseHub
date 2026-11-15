namespace EmployeeManagement.API.Models;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string EmployeesRead = "EmployeesRead";
    public const string EmployeesWrite = "EmployeesWrite";
    public const string MastersWrite = "MastersWrite";
}
