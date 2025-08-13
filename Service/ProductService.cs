using System.Text.Json;
using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;


namespace backend.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageService _productImageService;
        private readonly ICategoryService _categotyService;


        public ProductService(IProductRepository productRepository, IProductImageService productImageService, ICategoryService categoryService, IUserService userService)
        {
            _productRepository = productRepository;
            _productImageService = productImageService;
            _categotyService = categoryService;
        }

        public async Task<int> AddProduct(CreateProductDto dto, List<IFormFile> images)
        {
            _ = await _categotyService.GetCategoryByIdAsync(dto.CategoryId) ?? throw new NullReferenceException("category doesnot exist");
            var product = dto.ToEntity();
            var productId = await _productRepository.AddProduct(product);
            await _productImageService.AddProductImageAsync(images, productId);
            return productId;
        } 

        public async Task<bool> UpdateProduct(int id, UpdateProductDto dto, List<IFormFile> images)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null) return false;
            if (images.Count > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(images));
                var productImages = await _productImageService.GetImagesByProductIdAsync(id);
                foreach (var image in productImages)
                {
                    await _productImageService.DeleteProductImageAsync(id, image.ImageUrl);

                }
                await _productImageService.AddProductImageAsync(images, id);
            }



            product.UpdateEntity(dto);
            await _productRepository.UpdateProduct(id, product);
            return true;
        }

        public async Task DeleteProduct(int id)
        {
            var productImages = await _productImageService.GetImagesByProductIdAsync(id);
            foreach (var images in productImages)
            {
                await _productImageService.DeleteProductImageAsync(images.Id, images.ImageUrl);
            }
            await _productRepository.DeleteProduct(id);
        }

        public async Task<ProductReadDto?> GetProductById(int id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null) return null;
            var imageUrls = await _productImageService.GetImagesByProductIdAsync(id);
            var category = await _categotyService.GetCategoryByIdAsync(product.CategoryId)?? throw new KeyNotFoundException("Category Not Found");

            var productDto = product.ToReadDto([.. imageUrls.Select(e => e.ImageUrl)], category);


            return productDto;
        }

        public async Task<IEnumerable<ProductReadDto>> GetAllProducts()
        {
            var products = await _productRepository.GetAllProducts();
            List<ProductReadDto> productReadDtos = [];
            foreach (var product in products)
            {
                var image = await _productImageService.GetImagesByProductIdAsync(product.Id);
                var category = await _categotyService.GetCategoryByIdAsync(product.CategoryId)?? throw new KeyNotFoundException("Category Not Found");
                productReadDtos.Add(product.ToReadDto([.. image.Select(e => e.ImageUrl)], category));
            }
            return productReadDtos;
        }

        public async Task<List<ProductReadDto>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            var products = await _productRepository.SearchProductsAsync(name, minPrice, maxPrice, categoryId);
                            List<ProductReadDto> productReadDtos = [];
            foreach (var product in products)
            {
                var image = await _productImageService.GetImagesByProductIdAsync(product.Id);
                var category = await _categotyService.GetCategoryByIdAsync(product.CategoryId)?? throw new KeyNotFoundException("Category Not Found");
                productReadDtos.Add(product.ToReadDto([.. image.Select(e => e.ImageUrl)], category));
            }
            return productReadDtos;
            }
    }
}