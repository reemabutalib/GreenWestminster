using Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    // Enable detailed CORS logs for debugging
    logging.AddFilter("Microsoft.AspNetCore.Cors", LogLevel.Debug);
});

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? 
                    throw new InvalidOperationException("JWT key is not configured"))),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Initialize and seed the database before handling any requests
await InitializeDatabase(app);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline (order matters!)
app.UseCors("AllowReactApp");  // CORS should be before auth middleware

// Only use HTTPS redirection in production to avoid development issues
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseAuthentication();  // Authentication always before Authorization
app.UseAuthorization();

app.MapControllers();

// Weather forecast sample endpoint
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
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
.WithName("GetWeatherForecast")
.WithOpenApi();

// Set URL binding
app.Urls.Add("http://*:80");

await app.RunAsync();

// Database initialization method to keep the main flow clean
async Task InitializeDatabase(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing database...");
        var context = services.GetRequiredService<AppDbContext>();
        
        // Create the schema if needed
        await context.Database.EnsureCreatedAsync();
        
        // Check if data exists
        bool hasData = false;
        try
        {
            // Use parameterized query for better security
            var result = await context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM users");
            hasData = result > 0;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Tables not found or empty, will proceed with data seeding");
        }
        
        // Seed data if needed
        if (!hasData)
        {
            logger.LogInformation("Seeding database with initial data...");
            await DbSeeder.SeedData(context);
            logger.LogInformation("Database seeded successfully");
        }
        else
        {
            logger.LogInformation("Database already contains data, skipping seed");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        
        if (ex.InnerException != null)
        {
            logger.LogError(ex.InnerException, "Inner exception details");
        }
        
        // Optionally terminate the application on critical DB errors
        // application.Logger.LogCritical("Application cannot continue without database");
        // Environment.Exit(1);
    }
}

// Weather forecast record definition
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}