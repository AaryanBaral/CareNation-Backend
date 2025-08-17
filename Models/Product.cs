public enum ProductType
{
    Resale,
    Sell,
    Both

}

namespace backend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public int CategoryId { get; set; }
        public int VendorId { get; set; }  // FK to Vendors table

        public int StockQuantity { get; set; }
        public int RestockQuantity { get; set; }
        public int WarningStockQuantity { get; set; }

        public string Type { get; set; } = ProductType.Sell.ToString();

        public decimal ProductPoint { get; set; }

        public decimal Discount { get; set; }
        public decimal RepurchaseSale { get; set; }
        public decimal DistributorPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal CostPrice { get; set; }
    }
}
