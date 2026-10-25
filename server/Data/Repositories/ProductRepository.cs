using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;
using server.Common;
using server.DTOs;
using server.Extensions;
using server.Interfaces;

namespace eCommerceUoWBasic.Data.Repositories;

public sealed class ProductRepository(DataContext context) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(
        ProductQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);
        var sortBy = string.IsNullOrWhiteSpace(filter.SortBy)
            ? "Name, Id"
            : $"{filter.SortBy}, Id";

        IQueryable<Product> query = context.Products.AsNoTracking().ApplySearch(filter.Search);
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category.Trim();
            query = category == "__uncategorized__"
                ? query.Where(product => product.Category == "")
                : query.Where(product => product.Category == category);
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var products = await query
            .ApplySort(sortBy)
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

        return new PagedResponse<ProductDto>
        {
            Data = products,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await context.Products
            .AsNoTracking()
            .Where(product => product.Category != "")
            .Select(product => product.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);

        var hasUncategorizedProducts = await context.Products
            .AsNoTracking()
            .AnyAsync(product => product.Category == "", cancellationToken);
        if (hasUncategorizedProducts)
        {
            categories.Insert(0, "__uncategorized__");
        }

        return categories;
    }

    public async Task<bool> TryDecrementStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await context.Products
            .Where(product => product.Id == productId && product.StockQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    product => product.StockQuantity,
                    product => product.StockQuantity - quantity),
                cancellationToken);

        return affectedRows == 1;
    }

    public void Add(Product product) => context.Products.Add(product);

    public void AddRange(IEnumerable<Product> products) => context.Products.AddRange(products);

    public void Remove(Product product) => context.Products.Remove(product);
}
