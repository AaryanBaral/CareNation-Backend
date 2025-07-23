

using backend.Dto;
using backend.Models;

namespace backend.Interface.Service
{
    public interface IProductImageService
    {
        Task<List<int>> AddProductImageAsync(List<IFormFile> images, int productId);
        Task DeleteProductImageAsync(int id,string imageUrl);
        Task<List<ProductImage>> GetImagesByProductIdAsync(int productId);
    }
}