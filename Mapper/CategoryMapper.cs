
using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class CategoryMapper
    {
        // Map CreateCategoryDto to Category entity
        public static Category ToCategory(this CreateCategoryDto dto)
        {
            return new Category
            {
                Name = dto.Name
            };
        }

        // Map Category entity to ReadCategoryDto
        public static ReadCategoryDto ToReadDto(this Category category)
        {
            return new ReadCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        // Map UpdateCategoryDto to Category entity
        public static void UpdateFromDto(this Category category, UpdateCategoryDto dto)
        {
            category.Name = dto.Name;
        }
    }

}