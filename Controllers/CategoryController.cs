using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<ActionResult<SuccessResponseDto<int>>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var categoryId = await _categoryService.CreateCategoryAsync(dto);
            return Ok(new SuccessResponseDto<int> { Data = categoryId });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SuccessResponseDto<ReadCategoryDto>>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(new SuccessResponseDto<ReadCategoryDto> { Data = category });
        }

        [HttpGet]
        public async Task<ActionResult<SuccessResponseDto<IEnumerable<ReadCategoryDto>>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new SuccessResponseDto<IEnumerable<ReadCategoryDto>> { Data = categories });
        }

        [HttpPut]
        public async Task<ActionResult<SuccessResponseDto<string>>> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateCategoryAsync(dto);
            if (!updated) return NotFound();
            return Ok(new SuccessResponseDto<string> { Data = "Category updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SuccessResponseDto<string>>> DeleteCategory(int id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            if (!deleted) return NotFound();
            return Ok(new SuccessResponseDto<string> { Data = "Category deleted successfully" });
        }
    }
}
