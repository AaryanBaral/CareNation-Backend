using backend.Data;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly AppDbContext _context;

        public ProductImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> AddProductImageAsync(List<string> imageUrls, int productId)
        {
            var newIds = new List<int>();

            foreach (var imageUrl in imageUrls)
            {
                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrl
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync(); // Save after each insert to get the ID

                newIds.Add(productImage.Id);
            }

            return newIds;
        }

        public async Task DeleteProductImageAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image != null)
            {
                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ProductImage>> GetProductImagesByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .AsNoTracking()
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
        }
    }
}
