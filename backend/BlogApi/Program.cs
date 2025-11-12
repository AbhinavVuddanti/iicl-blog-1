using BlogApi.Data;
using BlogApi.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// CORS
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(
                    builder.Configuration["Cors:FrontendUrl"] ?? "http://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// EF Core DbContext (SQLite for development by default)
builder.Services.AddDbContext<BlogContext>(options =>
{
    var usePostgres = builder.Configuration.GetValue<bool>("UsePostgres");
    if (usePostgres)
    {
        var pg = builder.Configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrWhiteSpace(pg))
        {
            options.UseNpgsql(pg);
        }
        else
        {
            // Fallback to SQLite if Postgres connection is not configured in production
            var sqlite = builder.Configuration.GetConnectionString("Sqlite")
                          ?? "Data Source=blog.db";
            options.UseSqlite(sqlite);
        }
    }
    else
    {
        var sqlite = builder.Configuration.GetConnectionString("Sqlite")
                      ?? "Data Source=blog.db";
        options.UseSqlite(sqlite);
    }
});

// Rate limiting (fixed window)
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 100;
        options.QueueLimit = 0;
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(corsPolicy);
app.UseRateLimiter();

// Global error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

// Ensure database is created/migrated on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
        db.Database.Migrate();
    }
    catch (Exception)
    {
        // Let ErrorHandlingMiddleware capture runtime errors if any occur later
    }
}

// Map root for production so service root is not 404
if (!app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Json(new { service = "Blog API", status = "ok" }));
}

app.Run();
