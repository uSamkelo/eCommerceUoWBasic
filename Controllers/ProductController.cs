using eCommerceUoWBasic.Interfaces;
using eCommerceUoWBasic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceUoWBasic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IUnitofWork _unitofWork;

        public ProductController(ILogger<ProductController> logger, IUnitofWork unitofWork)
        {
            _logger = logger;
            _unitofWork = unitofWork;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                await _unitofWork.Products.AddAsync(product);
                await _unitofWork.CompleteAsync();

                return CreatedAtAction("GetItem", new {id = product.Id}, product);
            }

            return Problem();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var product = await _unitofWork.Products.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var products = await _unitofWork.Products.GetAllAsync();
            if (products == null) return NotFound();

            return Ok(products);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            await _unitofWork.Products.UpdateAsync(product);
            await _unitofWork.CompleteAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var product = await _unitofWork.Products.GetById(id);
            if(product == null)
            {
                return NotFound();
            }

            await _unitofWork.Products.DeleteAsync(id);
            await _unitofWork.CompleteAsync();

            return Ok();
        }
    }
}
