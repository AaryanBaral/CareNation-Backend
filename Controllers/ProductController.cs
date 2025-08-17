using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<ActionResult<SuccessResponseDto<int>>> AddProduct([FromForm]CreateProductDto dto, [FromForm] List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest("At least one image is required.");

            var productId = await _productService.AddProduct(dto, images);
            return Ok(new SuccessResponseDto<int> { Data = productId });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SuccessResponseDto<string>>> UpdateProduct(int id, [FromForm] UpdateProductDto dto, [FromForm] List<IFormFile>? images)
        {
            var success = await _productService.UpdateProduct(id, dto, images!);
            if (!success) return NotFound();
            return Ok(new SuccessResponseDto<string> { Data = "Product updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SuccessResponseDto<string>>> DeleteProduct(int id)
        {
            await _productService.DeleteProduct(id);
            return Ok(new SuccessResponseDto<string> { Data = "Product deleted successfully" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SuccessResponseDto<ProductReadDto>>> GetProductById(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();
            return Ok(new SuccessResponseDto<ProductReadDto> { Data = product });
        }

        [HttpGet]
        public async Task<ActionResult<SuccessResponseDto<IEnumerable<ProductReadDto>>>> GetAllProducts()
        {
            var products = await _productService.GetAllProducts();
            return Ok(new SuccessResponseDto<IEnumerable<ProductReadDto>> { Data = products });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? name,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? categoryId)
        {
            var products = await _productService.SearchProductsAsync(name, minPrice, maxPrice, categoryId);
            return Ok(products);
        }

    }
}
