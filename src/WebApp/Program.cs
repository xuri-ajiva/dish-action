using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using WebApp;
using WebApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {
    //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    BaseAddress = builder.Configuration["BaseUrl"] is { } baseAddress ? new Uri(baseAddress) : new Uri(new Uri(builder.HostEnvironment.BaseAddress), "data/")
});
builder.Services.AddAntDesign();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<DialogService>();

builder.Services.AddScoped<DishDashDataService>();

await builder.Build().RunAsync();
