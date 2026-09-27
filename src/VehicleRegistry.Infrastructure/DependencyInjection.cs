using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Infrastructure.Data;
using VehicleRegistry.Infrastructure.Services;

namespace VehicleRegistry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVehicleRegistryInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<VehicleRegistryDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VehicleRegistryDbContext>();
        await db.Database.MigrateAsync();
    }
}