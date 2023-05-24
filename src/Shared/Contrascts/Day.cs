public class Day
{
    public DateOnly Date { get; set; }
    public DayOfWeak DayOfWeak { get; set; }
    public string Source { get; set; } = null!;
    public string SourceName { get; set; } = null!;
    public List<Guid> Meals { get; set; } = null!;
}
