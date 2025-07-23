using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Interface.Repository
{
    public interface ICategoryRepository
    {
        Task<int> AddCategory(Category category);
        Task UpdateCategory(int id, Category category);
        Task DeleteCategory(int id);
        Task<Category?> GetCategoryById(int id);
        Task<IEnumerable<Category>> GetAllCategories();
    }
}