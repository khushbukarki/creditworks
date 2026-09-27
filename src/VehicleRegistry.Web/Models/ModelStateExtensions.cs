using Microsoft.AspNetCore.Mvc.ModelBinding;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Web.Models;

public static class ModelStateExtensions
{
    /// <summary>Shows service validation errors next to the matching form field.</summary>
    public static void AddValidationErrors(this ModelStateDictionary modelState, IEnumerable<ValidationError> errors, string prefix)
    {
        foreach (var error in errors)
        {
            var key = string.IsNullOrEmpty(error.Field) ? string.Empty
                : string.IsNullOrEmpty(prefix) ? error.Field
                : $"{prefix}.{error.Field}";
            modelState.AddModelError(key, error.Message);
        }
    }
}