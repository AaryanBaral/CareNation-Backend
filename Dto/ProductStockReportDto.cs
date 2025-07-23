public class ProductStockReportDto
{
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = "";
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool NeedsReorder { get; set; }
}
