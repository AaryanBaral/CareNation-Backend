
using backend.Models;

namespace backend.Interface.Repository
{
    public interface IProductRepository
    {
        Task<int> AddProduct(Product product);
        Task UpdateProduct(int id, Product product);
        Task DeleteProduct(int id);
        Task<Product?> GetProductById(int id);
        Task<IEnumerable<Product>> GetAllProducts();
            Task<List<Product>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId);
    }
}