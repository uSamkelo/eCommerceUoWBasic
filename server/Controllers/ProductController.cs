using eCommerceUoWBasic.Models;
using Microsoft.AspNetCore.Mvc;
using server.Common;
using server.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace eCommerceUoWBasic.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ProductController(
    IProductService productService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        Product product,
        CancellationToken cancellationToken)
    {
        await productService.CreateAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetItem), new { id = product.Id }, product);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetItem(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
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

        var outcome = await productService.UpdateAsync(product, cancellationToken);
        return outcome.Result switch
        {
            ProductUpdateResult.NotFound => NotFound(),
            ProductUpdateResult.Conflict => Conflict(new { message = "The product was modified by another request." }),
            ProductUpdateResult.Updated => Ok(outcome.Product),
            _ => throw new InvalidOperationException($"Unknown product update result: {outcome.Result}.")
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItem(
        int id,
        CancellationToken cancellationToken)
    {
        return await productService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
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

        var seededCount = await productService.SeedAsync(count, cancellationToken);
        return Ok(new { count = seededCount });
    }
}