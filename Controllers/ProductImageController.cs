using backend.Dto;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [HttpPost("{productId}")]
        public async Task<ActionResult<SuccessResponseDto<List<int>>>> UploadImages(int productId, [FromForm] List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest("No images provided.");

            var imageIds = await _productImageService.AddProductImageAsync(images, productId);
            return Ok(new SuccessResponseDto<List<int>> { Data = imageIds });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SuccessResponseDto<string>>> DeleteImage(int id, [FromQuery] string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return BadRequest("Image URL is required.");

            await _productImageService.DeleteProductImageAsync(id, imageUrl);
            return Ok(new SuccessResponseDto<string> { Data = "Image deleted successfully." });
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<SuccessResponseDto<List<ProductImage>>>> GetImagesByProductId(int productId)
        {
            var images = await _productImageService.GetImagesByProductIdAsync(productId);
            return Ok(new SuccessResponseDto<List<ProductImage>> { Data = images });
        }
    }
}
