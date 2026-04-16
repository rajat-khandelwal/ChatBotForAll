using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Auth;
using ChatBotForAll.ApiService.Repos;
using ChatBotForAll.ApiService.Services;
using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddDbContext<ChatBotDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("chatbotforall")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IDocumentRepository, EfDocumentRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IConversationRepository, EfConversationRepository>();
builder.Services.AddScoped<IMessageRepository, EfMessageRepository>();
builder.Services.AddScoped<IRagService, StubRagService>();
builder.Services.AddScoped<IChatService, ChatService>();

// OpenAPI + Scalar (replaces Swagger)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "ChatBotForAll API";
        document.Info.Version = "v1";
        document.Info.Description = "Multi-tenant RAG Chatbot Platform — Auth, Documents, Chat";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Auto-apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatBotDbContext>();
    await db.Database.MigrateAsync();

    // Seed default dev tenant + admin user if DB is empty
    if (!db.Tenants.Any())
    {
        var defaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var tenant = new Tenant
        {
            TenantId = defaultTenantId,
            TenantName = "Demo Tenant",
            Slug = "demo",
            IsActive = true,
            CreatedDateTime = DateTime.UtcNow,
            UpdatedDateTime = DateTime.UtcNow,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };
        db.Tenants.Add(tenant);

        var adminUser = new AppUser
        {
            UserId = Guid.NewGuid(),
            TenantId = defaultTenantId,
            UserName = "Admin",
            Email = "admin@demo.com",
            Role = UserRole.TenantAdmin,
            IsActive = true,
            CreatedDateTime = DateTime.UtcNow,
            UpdatedDateTime = DateTime.UtcNow,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");
        db.AppUsers.Add(adminUser);

        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ChatBotForAll API";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecurityScheme = JwtBearerDefaults.AuthenticationScheme
        };
    });
}

app.MapControllers();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
