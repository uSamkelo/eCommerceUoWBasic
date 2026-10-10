using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;
using server.Common;
using server.DTOs;
using server.Extensions;
using server.Interfaces;

namespace server.Services
{
    public sealed class ProuductService(DataContext dbContext) : IProuductService
    {
        public async Task<PagedResponse<ProductDto>> GetAllProductsAsync(
            ProductQueryFilter filter,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            IQueryable<Product> query = dbContext.Products.AsNoTracking();

            query = query.ApplySearch(filter.Search);

            var totalRecords = await query.CountAsync(cancellationToken);

            var sortBy = string.IsNullOrWhiteSpace(filter.SortBy)
                ? "Name, Id"
                : $"{filter.SortBy}, Id";
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

            return new PagedResponse<ProductDto>
            {
                Data = products,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
        }
    }
}