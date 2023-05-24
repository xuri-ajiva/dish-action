public class Meal
{
    public Guid Hash { get; set; }
    public string Name { get; set; } = null!;
    public Guid Image { get; set; }
    public MealCategory Category { get; set; }
    public AllergensAndAdditives AllergensAndAdditives { get; set; }
    public string Location { get; set; } = null!;
    public decimal Price { get; set; }
}
