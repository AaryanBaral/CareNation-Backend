namespace backend.Models;

public enum OrderStatus {
    Cancelled,
    Delivered,
    Pending
}

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public bool IsDeleted { get; set; }
                public bool IsRepurchase { get; set; }          // “true” if buyer had any prior Delivered order
        public decimal TotalPV { get; set; }   
}