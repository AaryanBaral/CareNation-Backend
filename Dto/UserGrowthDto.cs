public class UserGrowthDto
{
    public DateTime Period { get; set; }   // e.g., first day of the month or day
    public int TotalNewUsers { get; set; }
    public int TotalNewDistributors { get; set; }
}
