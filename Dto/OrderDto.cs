namespace backend.Dto
{
    public class OrderCreateDto
    {
        public List<CartItemDto> Items { get; set; } = new();
    }


    public class OrderUpdateDto
    {
        public required int Id { get; set; }
        public required string UserId { get; set; }
        public required decimal TotalAmount { get; set; }
    }

    public class OrderReadDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemReadDto> Items { get; set; } = new();
    }
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
