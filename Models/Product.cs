

namespace backend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public required decimal Price { get; set; }
        public required int StockQuantity { get; set; }
        public required int CategoryId { get; set; }
    }
}