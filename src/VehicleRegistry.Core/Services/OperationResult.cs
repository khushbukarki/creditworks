using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Core.Services;

public enum OperationStatus { Success, ValidationFailed, NotFound }

/// <summary>Expected failures are returned as values, not thrown as exceptions.</summary>
public sealed record OperationResult(OperationStatus Status, IReadOnlyList<ValidationError> Errors)
{
    public bool Succeeded => Status == OperationStatus.Success;

    public static OperationResult Success() => new(OperationStatus.Success, []);
    public static OperationResult NotFound() => new(OperationStatus.NotFound, []);
    public static OperationResult Invalid(IReadOnlyList<ValidationError> errors) => new(OperationStatus.ValidationFailed, errors);
    public static OperationResult Invalid(string field, string message) => Invalid([new ValidationError(field, message)]);
}