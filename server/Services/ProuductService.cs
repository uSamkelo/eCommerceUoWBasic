using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using server.Common;
using server.DTOs;
using server.Extensions;
using server.Interfaces;

namespace server.Services
{
    public sealed class ProuductService(
        DataContext dbContext,
        IMemoryCache cache,
        ProductCacheVersion cacheVersion) : IProuductService
    {
        public async Task<PagedResponse<ProductDto>> GetAllProductsAsync(
            ProductQueryFilter filter,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var sortBy = string.IsNullOrWhiteSpace(filter.SortBy)
                ? "Name, Id"
                : $"{filter.SortBy}, Id";
            var cacheKey = (
                cacheVersion.Current,
                "list",
                pageNumber,
                pageSize,
                sortBy,
                filter.Search,
                filter.Category?.Trim());

            if (cache.TryGetValue(
                cacheKey,
                out PagedResponse<ProductDto>? cachedResponse) &&
                cachedResponse is not null)
            {
                return cachedResponse;
            }

            IQueryable<Product> query = dbContext.Products.AsNoTracking();

            query = query.ApplySearch(filter.Search);
            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                var category = filter.Category.Trim();
                query = category == "__uncategorized__"
                    ? query.Where(product => product.Category == "")
                    : query.Where(product => product.Category == category);
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            query = query.ApplySort(sortBy);

            var products = await query
                .ApplyPagination(pageNumber, pageSize)
                .Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Category = product.Category,
                    StockQuantity = product.StockQuantity,
                    Version = product.Version
                })
                .ToListAsync(cancellationToken);

            var response = new PagedResponse<ProductDto>
            {
                Data = products,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                Size = 1
            });

            return response;
        }

        public async Task<IReadOnlyList<string>> GetProductCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = (cacheVersion.Current, "categories");
            if (cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cachedCategories) &&
                cachedCategories is not null)
            {
                return cachedCategories;
            }

            var categories = await dbContext.Products
                .AsNoTracking()
                .Where(product => product.Category != "")
                .Select(product => product.Category)
                .Distinct()
                .OrderBy(category => category)
                .ToListAsync(cancellationToken);

            var hasUncategorizedProducts = await dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Category == "", cancellationToken);
            if (hasUncategorizedProducts)
            {
                categories.Insert(0, "__uncategorized__");
            }

            cache.Set(cacheKey, categories, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                Size = 1
            });

            return categories;
        }
    }
}