using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRegistry.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel(IWebHostEnvironment environment) : PageModel
{
    public int? ErrorStatusCode { get; private set; }
    public string? RequestId { get; private set; }

    /// <summary>Technical details, shown only in Development, never to real users.</summary>
    public string? DeveloperDetails { get; private set; }

    public void OnGet(int? statusCode)
    {
        ErrorStatusCode = statusCode;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        if (environment.IsDevelopment())
        {
            var reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            DeveloperDetails =
                $"Status code: {statusCode?.ToString() ?? HttpContext.Response.StatusCode.ToString()}\n" +
                $"Original path: {reExecute?.OriginalPath ?? exception?.Path ?? "(unknown)"}\n" +
                $"Error: {exception?.Error.ToString() ?? "(no exception)"}";
        }
    }
}