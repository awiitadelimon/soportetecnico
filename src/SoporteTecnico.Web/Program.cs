using SoporteTecnico.Frontend;
using SoporteTecnico.Frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var mock = builder.Configuration.GetValue("UseMockData", true);
builder.Services.AddSingleton(new FrontendOptions(mock));
if (mock)
{
    builder.Services.AddScoped<ITf15Service, MockTf15Service>();
}
else
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        throw new InvalidOperationException("Configure ApiBaseUrl en wwwroot/appsettings.json.");

    builder.Services.AddScoped(_ => new HttpClient { BaseAddress = uri });
    builder.Services.AddScoped<ITf15Service, ApiTf15Service>();
}
await builder.Build().RunAsync();
