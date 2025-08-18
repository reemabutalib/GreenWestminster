using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Server.Data;
using Server.Repositories;
using Server.Repositories.Interfaces;
using Server.Services.Implementations;
using Server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Services ----------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (pooled)
builder.Services.AddDbContextPool<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// DI
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

builder.Services.AddHttpClient<IClimatiqService, ClimatiqService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
});

// CORS — allow your client and cache the preflight
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

// Process proxy headers from Render so the app knows the original scheme = https
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    // trust Render's proxy (we don't know its IPs here), so clear the defaults:
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

// JWT
var requireHttps = !builder.Environment.IsDevelopment();
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.SaveToken = true;
    o.RequireHttpsMetadata = requireHttps;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured"))),
        RoleClaimType = ClaimTypes.Role
    };
    o.Events = new JwtBearerEvents
    {
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode  = 401;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("""{"message":"You are not authorized"}""");
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", p =>
        p.RequireAssertion(ctx =>
            ctx.User.HasClaim(c => (c.Type == ClaimTypes.Role && c.Value == "Admin")
                                || (c.Type == "role"        && c.Value == "Admin"))));
});

// Response compression for JSON
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json" });
});

var app = builder.Build();

// ---------------- Middleware order (important) ----------------

// Process X-Forwarded-* from Render BEFORE anything that uses scheme/host
app.UseForwardedHeaders();

// Server-Timing so you can see backend time in DevTools
app.Use(async (ctx, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    ctx.Response.Headers.Append("Server-Timing", $"app;dur={sw.ElapsedMilliseconds}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();

// Routing → CORS → Auth → Authorization → Endpoints
app.UseRouting();
app.UseCors("AllowReactApp");

// IMPORTANT on Render: don't force HTTPS redirect unless ForwardedHeaders is in place.
// If you prefer to keep it, it must be AFTER UseForwardedHeaders (we did that) and BEFORE UseRouting.
// To avoid preflight redirects entirely, you can also comment the next line out.
if (requireHttps)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health checks (handy for warmers)
app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapGet("/healthz/db", async (AppDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok(new { ok = true }) : Results.Problem("db unreachable"));

// Bind to Render's PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// Apply migrations & warm EF
await InitializeDatabaseAsync(app);

await app.RunAsync();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying migrations…");
        await db.Database.MigrateAsync();

        // Warm-up query builds EF model & opens a pooled connection
        _ = await db.Users.AsNoTracking().AnyAsync();

        logger.LogInformation("Database ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}
