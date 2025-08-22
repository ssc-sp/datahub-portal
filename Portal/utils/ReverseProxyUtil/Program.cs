using Datahub.Application.Services.ReverseProxy;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Services.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


// Register reverse proxy services
builder.Services.AddReverseProxy().AddTransformFactory<URLTranslationTransformFactory>();

// Provide a simple config service for the utility and wire up the proxy config provider
builder.Services.AddSingleton<IReverseProxyConfigService, ReverseProxyUtil.SimpleReverseProxyConfig>();
builder.Services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapReverseProxy();       

app.Run();
