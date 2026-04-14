using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using CodeTicket.Frontend;
using CodeTicket.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.IsProduction()
        ? "https://codeticket-production.up.railway.app"
        : "http://localhost:5007")
});

builder.Services.AddScoped<ApiService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
