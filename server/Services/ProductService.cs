using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using server.Common;
using server.DTOs;
using server.Interfaces;

namespace server.Services;

public sealed class ProductService(
    IUnitOfWork unitOfWork,
    IMemoryCache cache,
    ProductCacheVersion cacheVersion) : IProductService
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = (cacheVersion.Current, "detail", id);
        if (cache.TryGetValue(cacheKey, out Product? cachedProduct))
        {
            return cachedProduct;
        }

        var product = await unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is not null)
        {
            cache.Set(cacheKey, product, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20),
                Size = 1
            });
        }

        return product;
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        unitOfWork.Products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        cacheVersion.Invalidate();
        return product;
    }

    public async Task<ProductUpdateOutcome> UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.Products.GetTrackedByIdAsync(product.Id, cancellationToken);
        if (existing is null)
        {
            return new ProductUpdateOutcome(ProductUpdateResult.NotFound);
        }

        existing.Name = product.Name;
        existing.Price = product.Price;
        existing.Category = product.Category;
        existing.StockQuantity = product.StockQuantity;

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ProductUpdateOutcome(ProductUpdateResult.Conflict);
        }

        cacheVersion.Invalidate();
        return new ProductUpdateOutcome(ProductUpdateResult.Updated, existing);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetTrackedByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        unitOfWork.Products.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        cacheVersion.Invalidate();
        return true;
    }

    public async Task<int> SeedAsync(int count, CancellationToken cancellationToken = default)
    {
        var random = Random.Shared;
        var products = Enumerable.Range(1, count)
            .Select(index => new Product
            {
                Name = $"Product {index}",
                Price = Math.Round((decimal)(random.NextDouble() * 500 + 1), 2),
                StockQuantity = random.Next(1, 11)
            })
            .ToList();

        unitOfWork.Products.AddRange(products);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        cacheVersion.Invalidate();
        return products.Count;
    }

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

        if (cache.TryGetValue(cacheKey, out PagedResponse<ProductDto>? cachedResponse) &&
            cachedResponse is not null)
        {
            return cachedResponse;
        }

        var response = await unitOfWork.Products.GetPagedAsync(filter, cancellationToken);
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

        var categories = await unitOfWork.Products.GetCategoriesAsync(cancellationToken);
        cache.Set(cacheKey, categories, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
            Size = 1
        });

        return categories;
    }
}
