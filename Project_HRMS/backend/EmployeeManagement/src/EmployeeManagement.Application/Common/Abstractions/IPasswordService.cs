namespace EmployeeManagement.Application.Common.Abstractions;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}
