using eCommerceUoWBasic.Models;
using server.Common;
using server.DTOs;

namespace server.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductDto>> GetPagedAsync(
        ProductQueryFilter filter,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> TryDecrementStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);
    void Add(Product product);
    void AddRange(IEnumerable<Product> products);
    void Remove(Product product);
}
