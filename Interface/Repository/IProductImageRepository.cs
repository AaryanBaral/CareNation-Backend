
using backend.Models;

namespace backend.Interface.Repository
{
    public interface IProductImageRepository
    {
        Task<List<int>> AddProductImageAsync(List<string> imageUrls, int productId);
        Task DeleteProductImageAsync(int id);
        Task<List<ProductImage>> GetProductImagesByProductIdAsync(int productId);
    }
}