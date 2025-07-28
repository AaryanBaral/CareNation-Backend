namespace backend.Dto;

public class CartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
public class CartReadDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItemDetailsDto> Items { get; set; } = new();
}
    public class UpdateQuantityDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

public class CartItemDetailsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

