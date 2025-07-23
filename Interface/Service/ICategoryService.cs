using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dto;

namespace backend.Interface.Service
{
    public interface ICategoryService
    {
        Task<int> CreateCategoryAsync(CreateCategoryDto dto);
        Task<ReadCategoryDto?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<ReadCategoryDto>> GetAllCategoriesAsync();
        Task<bool> UpdateCategoryAsync(UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}