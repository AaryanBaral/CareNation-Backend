using backend.Dto;
using backend.Mapper;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;


namespace backend.Service
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _repository;

        public ProductImageService(IProductImageRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<int>> AddProductImageAsync(List<IFormFile> images, int productId)
        {
            if (images == null || images.Count == 0) throw new ArgumentNullException("No images provided");
            List<string> imageUrls = [];
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            Directory.CreateDirectory(uploadsFolder);
            foreach (var image in images)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                imageUrls.Add($"/images/{fileName}");
            }
            return await _repository.AddProductImageAsync(imageUrls, productId);
        }


        public async Task DeleteProductImageAsync(int id, string imageUrl)
        {
            var cleanedPath = imageUrl.TrimStart('/', '\\');
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanedPath);
            Console.WriteLine($"filePath of image is {filePath}");
            if (File.Exists(filePath))
            {
                Console.WriteLine(filePath);
                File.Delete(filePath); // delete the physical file
                Console.WriteLine("File deleted");
            }
            else
            {
                Console.WriteLine("No file exists");
            }
            await _repository.DeleteProductImageAsync(id);
        }

        public async Task<List<ProductImage>> GetImagesByProductIdAsync(int productId)
        {
            var images = await _repository.GetProductImagesByProductIdAsync(productId);
            return images;
        }
    }
}
 