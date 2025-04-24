using Azure.Identity;
using GymSystem.API.Extentions;
using GymSystem.API.MiddleWares;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Repositories.Business;
using GymSystem.BLL.Services;
using GymSystem.BLL.Services.Business;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities.Identity;
using GymSystem.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUrl = "https://GymSystemKeyVault.vault.azure.net/"; 
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUrl),
    new DefaultAzureCredential());

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; 
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add DbContext with respect to the SoftDeleteInterceptor
builder.Services.AddDbContext<AppIdentityDbContext>(
    (serviceProvider, options) => options
    .UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"))
    .AddInterceptors(serviceProvider.GetRequiredService<SoftDeleteInterceptor>()));

// Allow Dependency Injection for Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrEmpty(redisConnectionString))
    {
        throw new InvalidOperationException("Redis connection string is missing in configuration.");
    }

    var redisConfig = ConfigurationOptions.Parse(redisConnectionString, true);
    return ConnectionMultiplexer.Connect(redisConfig);
});


builder.Services.AddHttpClient<IGenerativeAIService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    
    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        if (errors == System.Net.Security.SslPolicyErrors.None)
            return true;

        Console.WriteLine($"SSL Certificate error: {errors}");
        return false; 
    }
});

builder.Services.AddSingleton<IGenerativeAIService>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["GoogleApiKey"];
    var cache = provider.GetRequiredService<IDistributedCache>();
    var validator = provider.GetRequiredService<PlanValidator>();
    return new GeminiService(httpClient, apiKey, cache, validator);
});
// Register application services
builder.Services.AddApplicationServices();

// Register identity services
builder.Services.AddIdentityServices(builder.Configuration);
//builder.Services.AddSwaggerService();
builder.Services.AddMemoryCache();
builder.Services.AddLogging();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

builder.Services.AddLogging();
var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseMiddleware<ExceptionMiddleWare>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var loggerFactory = services.GetRequiredService<ILoggerFactory>();

try
{
    var identityContext = services.GetRequiredService<AppIdentityDbContext>();
    await identityContext.Database.MigrateAsync();

	var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await AppIdentityDbContextSeed.SeedAsync(userManager);
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred during migration or seeding.");
}

app.Run();