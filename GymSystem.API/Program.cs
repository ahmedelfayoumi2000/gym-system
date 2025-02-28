using GymSystem.API.Extentions;
using GymSystem.API.MiddleWares;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities.Identity;
using GymSystem.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow Dependency Injection for StoreContext
//builder.Services.AddDbContext<GymSystemContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});

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
        // Allow all origins, headers, and methods during development
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();

        // Uncomment the line below to restrict access to specific origins
        // policy.WithOrigins("https://example.com")
        //       .AllowAnyHeader()
        //       .AllowAnyMethod();
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

    //var context = services.GetRequiredService<GymSystemContext>();
    //await context.Database.MigrateAsync();

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