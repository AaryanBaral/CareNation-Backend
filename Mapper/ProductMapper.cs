using System;

using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class ProductMapper
    {
        public static ProductReadDto ToReadDto(
            this Product product,
            List<string> imageUrl,
            ReadCategoryDto category,
            string vendorName = "")
        {
            return new ProductReadDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Category = category,
                VendorId = product.VendorId,
                VendorName = vendorName,
                StockQuantity = product.StockQuantity,
                RestockQuantity = product.RestockQuantity,
                WarningStockQuantity = product.WarningStockQuantity,
                Type = product.Type,
                ProductPoint = product.ProductPoint,
                Discount = product.Discount,
                RepurchaseSale = product.RepurchaseSale,
                DistributorPrice = product.DistributorPrice,
                RetailPrice = product.RetailPrice,
                CostPrice = product.CostPrice,
                ImageUrl = imageUrl
            };
        }

        public static Product ToEntity(this CreateProductDto dto)
        {
            if (dto == null) return null!;
            return new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                VendorId = dto.VendorId,
                StockQuantity = dto.StockQuantity,
                RestockQuantity = dto.RestockQuantity,
                WarningStockQuantity = dto.WarningStockQuantity,
                Type = dto.Type,
                ProductPoint = dto.ProductPoint,
                Discount = dto.Discount,
                RepurchaseSale = dto.RepurchaseSale,
                DistributorPrice = dto.DistributorPrice,
                RetailPrice = dto.RetailPrice,
                CostPrice = dto.CostPrice
            };
        }

        public static void UpdateEntity(this Product product, UpdateProductDto dto)
        {
            if (product == null || dto == null) return;

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.VendorId = dto.VendorId;
            product.StockQuantity = dto.StockQuantity;
            product.RestockQuantity = dto.RestockQuantity;
            product.WarningStockQuantity = dto.WarningStockQuantity;
            product.Type = dto.Type;
            product.ProductPoint = dto.ProductPoint;
            product.Discount = dto.Discount;
            product.RepurchaseSale = dto.RepurchaseSale;
            product.DistributorPrice = dto.DistributorPrice;
            product.RetailPrice = dto.RetailPrice;
            product.CostPrice = dto.CostPrice;
        }
    }

}