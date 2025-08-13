using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.Id;
        }

        public async Task UpdateProduct(int id, Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _context.Products
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<Product>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId)
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(p => p.Title.ToLower().Contains(name.ToLower()));

                if (minPrice.HasValue)
                    query = query.Where(p => p.RetailPrice >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(p => p.RetailPrice <= maxPrice.Value);

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                return await query.ToListAsync();
            }
    }
}
