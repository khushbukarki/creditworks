namespace VehicleRegistry.Core.Rules;

/// <summary>A user-facing validation message. Empty Field = applies to the whole form.</summary>
public sealed record ValidationError(string Field, string Message);