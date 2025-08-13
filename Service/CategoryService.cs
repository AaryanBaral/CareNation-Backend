
using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;

namespace backend.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        

        public async Task<int> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = dto.ToCategory();
            return await _categoryRepository.AddCategory(category);
        }

        public async Task<ReadCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
                return null;

            return category.ToReadDto();
        }

        public async Task<IEnumerable<ReadCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategories();
            return [.. categories.Select(e => e.ToReadDto())];
        }

        public async Task<bool> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            var existingCategory = await _categoryRepository.GetCategoryById(dto.Id);
            if (existingCategory == null)
                return false;

            existingCategory.UpdateFromDto(dto);
            await _categoryRepository.UpdateCategory(dto.Id, existingCategory);
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetCategoryById(id);
            if (existingCategory == null)
                return false;

            await _categoryRepository.DeleteCategory(id);
            return true;
        }

    }
}