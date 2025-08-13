

namespace backend.Dto
{
    public class CreateProductDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int VendorId { get; set; }  // FK to Vendors table

        public int StockQuantity { get; set; }

        public int RestockQuantity { get; set; }

        public int WarningStockQuantity { get; set; }

        /// <summary>
        /// Type of product: "retail", "resale", or "both"
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public decimal ProductPoint { get; set; }

        public decimal Discount { get; set; }

        public decimal RepurchaseSale { get; set; }

        public decimal DistributorPrice { get; set; }

        public decimal RetailPrice { get; set; }

        public decimal CostPrice { get; set; }
    }
    public class UpdateProductDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int VendorId { get; set; }
        public int StockQuantity { get; set; }
        public int RestockQuantity { get; set; }
        public int WarningStockQuantity { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal ProductPoint { get; set; }
        public decimal Discount { get; set; }
        public decimal RepurchaseSale { get; set; }
        public decimal DistributorPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal CostPrice { get; set; }
    }

    public class ProductReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public ReadCategoryDto Category { get; set; } = null!;
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty; // Optional, if you want to show name directly
        public int StockQuantity { get; set; }
        public int RestockQuantity { get; set; }
        public int WarningStockQuantity { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal ProductPoint { get; set; }
        public decimal Discount { get; set; }
        public decimal RepurchaseSale { get; set; }
        public decimal DistributorPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal CostPrice { get; set; }
        public List<string> ImageUrl { get; set; } = new();
    }

}