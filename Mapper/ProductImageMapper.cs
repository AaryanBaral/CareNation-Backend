using backend.Models;
using backend.Dto;

namespace backend.Mapper
{
    public static class ProductImageMapper
    {

        public static ProductImageReadDto ToReadDto(this ProductImage image)
        {
            return new ProductImageReadDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl
            };
        }
    }
}
