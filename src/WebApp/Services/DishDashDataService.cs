using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace WebApp.Services;

public class DishDashDataService
{
    private readonly HttpClient _client;
    private List<Day>? _days;
    private List<Image>? _images;
    private List<Meal>? _meals;
    private List<string>? _sources;
    public DishDashDataService(HttpClient client) => _client = client;

    [MemberNotNull(nameof(_days), nameof(_images), nameof(_meals))]
    public async Task TryFetchDataAsync()
    {
#pragma warning disable CS8774
        if (HasData) return;
#pragma warning restore CS8774
        await FetchInternal();
    }
    [MemberNotNull(nameof(_days), nameof(_images), nameof(_meals))]
    private async Task FetchInternal()
    {
        _days = await _client.GetFromJsonAsync<List<Day>>("days.json") ?? new List<Day>();
        _images = await _client.GetFromJsonAsync<List<Image>>("images.json") ?? new List<Image>();
        _meals = await _client.GetFromJsonAsync<List<Meal>>("meals.json") ?? new List<Meal>();
        HasData = true;
    }

    public bool HasData { get; private set; }

    public async Task<List<string>> GetAllSources()
    {
        await TryFetchDataAsync();
        return _sources ??= _days.Select(x => x.Source).Distinct().ToList();
    }

    public async Task<List<Day>> GetDayByRange(DateOnly start, DateOnly end)
    {
        await TryFetchDataAsync();
        return _days.Where(x => x.Date >= start && x.Date <= end).ToList();
    }

    public async Task<List<Meal>> GetAllMeals()
    {
        await TryFetchDataAsync();
        return _meals.ToList();
    }
}
