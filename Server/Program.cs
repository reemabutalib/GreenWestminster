using Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS - Define once
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// Database initialization should happen BEFORE using the app
// This ensures the database is ready before any requests come in

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Initializing database...");
        
        try
        {
            // CRITICAL FIX: Force recreate the schema with proper table names
            // Comment out after successful creation to avoid data loss on restarts
            await context.Database.EnsureCreatedAsync();
            
            // Explicitly check if tables are populated
            bool hasData = false;
            
            try
            {
                // Try direct SQL query with lowercase table name
                var result = await context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM users");
                hasData = result > 0;
            }
            catch (Exception)
            {
                logger.LogInformation("Tables not found or empty, seeding data...");
                hasData = false;
            }
            
            if (!hasData)
            {
                logger.LogInformation("Seeding database with initial data...");
                await DbSeeder.SeedData(context);
                logger.LogInformation("Database seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during schema creation or data seeding");
            throw; // Rethrow to be caught by outer try-catch
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        Console.WriteLine($"Database initialization error: {ex.Message}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            logger.LogError(ex.InnerException, "Inner exception details");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Use CORS middleware - Use the named policy
app.UseCors("AllowReactApp");

// Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers after middleware
app.MapControllers();

// Sample endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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
.WithName("GetWeatherForecast")
.WithOpenApi();

// Set URL at the end before running
app.Urls.Add("http://*:80");

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}