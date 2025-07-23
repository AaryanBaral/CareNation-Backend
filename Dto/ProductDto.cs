

namespace backend.Dto
{
    public class CreateProductDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public required int StockQuantity { get; set; }
    }
    public class UpdateProductDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public required int StockQuantity { get; set; }
    }
    public class ProductReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public required ReadCategoryDto Category { get; set; }
        public required int StockQuantity { get; set; }
        public required List<string> ImageUrl { get; set; }
    }
}