namespace backend.Dto
{

    public class ProductImageReadDto
    {
        public required int Id { get; set; }
        public required int ProductId { get; set; }
        public required string ImageUrl { get; set; }
    }
}
