using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Server.Data;
using Server.Repositories;
using Server.Repositories.Interfaces;
using Server.Services.Implementations;
using Server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

// Controllers & minimal OpenAPI (Swagger only in dev)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging (console/debug is fine; keep noise low in prod)
builder.Services.AddLogging();

// DbContext *pooling* + Npgsql
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// App services & repos
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IActivitiesService, ActivitiesService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IChallengesService, ChallengesService>();
builder.Services.AddScoped<IRolesService, RolesService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActivityCompletionRepository, ActivityCompletionRepository>();
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<ISustainableActivityRepository, SustainableActivityRepository>();
builder.Services.AddScoped<ISustainableEventRepository, SustainableEventRepository>();

// Outbound HTTP client(s) with sensible timeouts
builder.Services.AddHttpClient<IClimatiqService, ClimatiqService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
});

// CORS — cache preflight to avoid OPTIONS on every call
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "https://greenwestminster-client.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(12));
    });
});

// JWT auth
var requireHttps = !builder.Environment.IsDevelopment();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = requireHttps;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured"))),

        // Make sure roles resolve correctly
        RoleClaimType = ClaimTypes.Role
    };

    // Return 401 JSON instead of redirect
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("""{"message":"You are not authorized"}""");
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(c => (c.Type == ClaimTypes.Role && c.Value == "Admin")
                                || (c.Type == "role"        && c.Value == "Admin"))));
});

// Response compression (helps those post-login JSON calls)
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<GzipCompressionProvider>();
    o.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json" });
});

// ---------- App ----------

var app = builder.Build();

// Server-Timing: see total backend time in the browser’s Network → Timing
app.Use(async (ctx, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    ctx.Response.Headers.Append("Server-Timing", $"app;dur={sw.ElapsedMilliseconds}");
});

// Swagger only in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();
app.UseCors("AllowReactApp");

if (requireHttps)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoints (good for uptime pings / cold-starts)
app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapGet("/healthz/db", async (AppDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok(new { ok = true }) : Results.Problem("db unreachable"));

// Bind to Render’s provided PORT if present (more robust than forcing :80)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// Apply migrations & warm the EF model/connection pool before serving traffic
await InitializeDatabaseAsync(app);

await app.RunAsync();


// ---------- Helpers ----------

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying migrations…");
        await db.Database.MigrateAsync();

        // Warm up: build EF model & open a pooled connection
        _ = await db.Users.AsNoTracking().AnyAsync();

        logger.LogInformation("Database ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}
