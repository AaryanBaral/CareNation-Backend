namespace backend.Dto
{
    public class OrderItemCreateDto
    {
        public required int OrderId { get; set; }
        public required int ProductId { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
    }
    public class OrderItemUpdateDto
    {
        public required int Id { get; set; }
        public required int OrderId { get; set; }
        public required int ProductId { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
    }
        public class OrderItemReadDto
    {
        public required int Id { get; set; }
        public required int OrderId { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
    }
}