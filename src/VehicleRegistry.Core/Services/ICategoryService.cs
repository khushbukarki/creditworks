using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Core.Services;

public sealed record CategoryInput(string? Name, string? IconKey, decimal? MinWeightKg);

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryRange>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryRange?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CategoryInput input, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(int id, CategoryInput input, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}