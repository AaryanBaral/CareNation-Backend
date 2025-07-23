

using backend.Dto;

namespace backend.Interface.Service
{
    public interface IProductService
    {
        Task<int> AddProduct(CreateProductDto dto, List<IFormFile> images);
        Task<bool> UpdateProduct(int id, UpdateProductDto dto, List<IFormFile> images);
        Task DeleteProduct(int id);
        Task<ProductReadDto?> GetProductById(int id);
        Task<IEnumerable<ProductReadDto>> GetAllProducts();
        Task<List<ProductReadDto>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId);
    }
}