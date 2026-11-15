using FluentValidation;
using FluentValidation.Results;

namespace EmployeeManagement.Application.Common.Exceptions;

public sealed class NotFoundException(string entity, object key)
    : Exception($"{entity} with id '{key}' was not found.");

public sealed class ConflictException(string message) : Exception(message);

public sealed class UnauthorizedException(string message) : Exception(message);

public static class Fail
{
    /// <summary>Creates a FluentValidation exception for a single failed property.</summary>
    public static ValidationException Validation(string property, string message) =>
        new([new ValidationFailure(property, message)]);
}
