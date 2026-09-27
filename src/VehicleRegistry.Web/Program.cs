using System.Globalization;
using Microsoft.AspNetCore.Localization;
using VehicleRegistry.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("VehicleRegistry");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Connection string 'VehicleRegistry' is not configured. See 'Configure the database' in README.md.");

builder.Services.AddRazorPages();
builder.Services.AddVehicleRegistryInfrastructure(connectionString);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // no stack traces for users (§14)
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
app.UseHttpsRedirection();

// Decimals always use '.', e.g. 1850.75
var culture = new CultureInfo("en-NZ");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = [culture],
    SupportedUICultures = [culture],
});

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    await app.Services.ApplyMigrationsAsync();

app.Run();