using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Common;
using server.Interfaces;
using server.Services;

namespace eCommerceUoWBasic.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductController(
    DataContext context,
    IProuductService productService,
    IMemoryCache cache,
    ProductCacheVersion cacheVersion) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        Product product,
        CancellationToken cancellationToken)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
        cacheVersion.Invalidate();

        return CreatedAtAction(nameof(GetItem), new { id = product.Id }, product);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetItem(
        int id,
        CancellationToken cancellationToken)
    {
        var cacheKey = (cacheVersion.Current, "detail", id);
        if (!cache.TryGetValue(cacheKey, out Product? product))
        {
            product = await context.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

            if (product is not null)
            {
                cache.Set(cacheKey, product, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                    Size = 1
                });
            }
        }

        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedItems([FromQuery] ProductQueryFilter filter, CancellationToken cancellationToken)
    {
        var pagedResponse = await productService.GetAllProductsAsync(filter, cancellationToken);
        return Ok(pagedResponse);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await productService.GetProductCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItem(
        int id,
        Product product,
        CancellationToken cancellationToken)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        var existing = await context.Products
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = product.Name;
        existing.Price = product.Price;
        existing.Category = product.Category;
        existing.StockQuantity = product.StockQuantity;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "The product was modified by another request." });
        }

        cacheVersion.Invalidate();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItem(
        int id,
        CancellationToken cancellationToken)
    {
        var affectedRows = await context.Products
            .Where(product => product.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (affectedRows > 0)
        {
            cacheVersion.Invalidate();
        }

        return affectedRows == 0 ? NotFound() : NoContent();
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedProducts(
        [FromQuery] int count = 200,
        CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 10_000)
        {
            return BadRequest(new { message = "Count must be between 1 and 10,000." });
        }

        var random = Random.Shared;
        var products = Enumerable.Range(1, count)
            .Select(index => new Product
            {
                Name = $"Product {index}",
                Price = Math.Round((decimal)(random.NextDouble() * 500 + 1), 2),
                StockQuantity = random.Next(1, 11)
            })
            .ToList();

        context.Products.AddRange(products);
        await context.SaveChangesAsync(cancellationToken);
        cacheVersion.Invalidate();

        return Ok(new { count = products.Count });
    }
}