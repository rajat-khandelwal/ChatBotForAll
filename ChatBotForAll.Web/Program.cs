using ChatBotForAll.Web;
using ChatBotForAll.Web.Auth;
using ChatBotForAll.Web.Components;
using ChatBotForAll.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
//builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

// Auth
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();


// API Clients — all point to the API service via Aspire service discovery
builder.Services.AddHttpClient<AuthApiClient>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<DocumentApiClient>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<ChatApiClient>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

//app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
