public class SystemCounter
{
    public string Name { get; set; } = default!;   // PK, e.g. "UserId"
    public long NextValue { get; set; }            // next number to issue (start at 1)
}