using eCommerceUoWBasic.Models;
using server.Common;
using server.DTOs;

namespace server.Interfaces;

public interface IProductService
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task<ProductUpdateOutcome> UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SeedAsync(int count, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductDto>> GetAllProductsAsync(
        ProductQueryFilter filter,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetProductCategoriesAsync(
        CancellationToken cancellationToken = default);
}

public enum ProductUpdateResult
{
    Updated,
    NotFound,
    Conflict
}

public sealed record ProductUpdateOutcome(ProductUpdateResult Result, Product? Product = null);
