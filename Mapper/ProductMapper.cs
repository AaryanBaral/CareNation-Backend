using System;

using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class ProductMapper
    {
        public static ProductReadDto ToReadDto(this Product product, List<string> imageUrl, ReadCategoryDto category)
        {


            return new ProductReadDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ImageUrl = imageUrl,
                StockQuantity = product.StockQuantity,
                Category = category
            };
        }

        public static Product ToEntity(this CreateProductDto dto)
        {
            if (dto == null) return null!;

            return new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                StockQuantity = dto.StockQuantity
            };
        }

        public static void UpdateEntity(this Product product, UpdateProductDto dto)
        {
            if (product == null || dto == null) return;

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.StockQuantity = dto.StockQuantity;
        }

    }
}