using server.Common;
using server.DTOs;

namespace server.Interfaces
{
    public interface IProuductService
    {
        Task<PagedResponse<ProductDto>> GetAllProductsAsync(ProductQueryFilter filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetProductCategoriesAsync(CancellationToken cancellationToken = default);
    }
}