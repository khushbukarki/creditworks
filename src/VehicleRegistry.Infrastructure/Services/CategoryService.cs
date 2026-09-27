using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Rules;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Infrastructure.Data;

namespace VehicleRegistry.Infrastructure.Services;

public sealed class CategoryService(VehicleRegistryDbContext db, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryRange>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await db.WeightCategories.AsNoTracking().ToListAsync(cancellationToken);
        return CategoryRules.ToRanges(categories);
    }

    public async Task<CategoryRange?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ranges = await GetCategoriesAsync(cancellationToken);
        return ranges.FirstOrDefault(r => r.Id == id);
    }

    public Task<OperationResult> CreateAsync(CategoryInput input, CancellationToken cancellationToken = default)
    {
        var inputErrors = ValidateInput(input);
        if (inputErrors.Count > 0)
            return Task.FromResult(OperationResult.Invalid(inputErrors));

        return ChangeConfigurationAsync(categories =>
        {
            var category = new WeightCategory();
            ApplyInput(input, category);
            categories.Add(category);
            db.WeightCategories.Add(category);
            return null;
        }, cancellationToken);
    }

    public Task<OperationResult> UpdateAsync(int id, CategoryInput input, CancellationToken cancellationToken = default)
    {
        var inputErrors = ValidateInput(input);
        if (inputErrors.Count > 0)
            return Task.FromResult(OperationResult.Invalid(inputErrors));

        return ChangeConfigurationAsync(categories =>
        {
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category is null)
                return OperationResult.NotFound();

            ApplyInput(input, category);
            return null;
        }, cancellationToken);
    }

    public Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        ChangeConfigurationAsync(categories =>
        {
            var ordered = categories.OrderBy(c => c.MinWeightKg).ToList();
            var index = ordered.FindIndex(c => c.Id == id);
            if (index < 0)
                return OperationResult.NotFound();

            var absorbingIndex = CategoryRules.AbsorbingIndex(index, ordered.Count);
            if (absorbingIndex is null)
                return OperationResult.Invalid(string.Empty,
                    "You can't delete the only category. Every vehicle must belong to a category.");

            var deleted = ordered[index];

            // Deleting the lightest: the next one extends down to 0 kg, so there is no gap.
            if (index == 0)
                ordered[absorbingIndex.Value].MinWeightKg = deleted.MinWeightKg;

            categories.Remove(deleted);
            db.WeightCategories.Remove(deleted);
            return null;
        }, cancellationToken);

    /// <summary>
    /// Load all categories, apply one change, validate the WHOLE result, save in one transaction.
    /// Serializable isolation stops two concurrent edits combining into an invalid configuration.
    /// </summary>
    private async Task<OperationResult> ChangeConfigurationAsync(
        Func<List<WeightCategory>, OperationResult?> applyChange,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var categories = await db.WeightCategories.ToListAsync(cancellationToken);

        var earlyResult = applyChange(categories);
        if (earlyResult is not null)
        {
            db.ChangeTracker.Clear();
            return earlyResult;
        }

        var errors = CategoryRules.ValidateConfiguration(categories);
        if (errors.Count > 0)
        {
            db.ChangeTracker.Clear();
            return OperationResult.Invalid(errors);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return OperationResult.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Saving the category configuration failed.");
            db.ChangeTracker.Clear();
            return OperationResult.Invalid(string.Empty,
                "The categories couldn't be saved because they were changed at the same time by someone else. Reload the page and try again.");
        }
    }

    private static List<ValidationError> ValidateInput(CategoryInput input)
    {
        var errors = CategoryRules.ValidateCategory(input.Name, input.IconKey, input.MinWeightKg ?? 0m).ToList();
        if (input.MinWeightKg is null)
            errors.Add(new(nameof(CategoryInput.MinWeightKg), "Starting weight is required."));
        return errors;
    }

    private static void ApplyInput(CategoryInput input, WeightCategory category)
    {
        category.Name = input.Name!.Trim();
        category.IconKey = input.IconKey!;
        category.MinWeightKg = input.MinWeightKg!.Value;
    }
}