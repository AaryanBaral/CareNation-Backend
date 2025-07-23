namespace backend.Dto;
public class SalesByProductDto
{
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = "";
    public int TotalQuantitySold { get; set; }
    public decimal TotalSales { get; set; }
}

// Sales by Category
public class SalesByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int TotalQuantitySold { get; set; }
    public decimal TotalSales { get; set; }
}

// Sales by Time
public class SalesByTimeDto
{
    public DateTime Date { get; set; } // Or use string for Month/Year granularity
    public decimal TotalSales { get; set; }
}


