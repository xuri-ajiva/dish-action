public class Meal
{
    public Guid Hash { get; set; }
    public string Name { get; set; }
    public Guid Image { get; set; }
    public MealCategory Category { get; set; }
    public AllergensAndAdditives AllergensAndAdditives { get; set; }
    public string Location { get; set; }
    public decimal Price { get; set; }
}