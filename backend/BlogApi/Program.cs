using BlogApi.Data;
using BlogApi.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// CORS: TEMP allow all to unblock immediately
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// EF Core DbContext (SQLite or Postgres)
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
            // Fallback to SQLite if Postgres is not configured
            var sqlite = builder.Configuration.GetConnectionString("Sqlite")
                          ?? "Data Source=blog.db";
            options.UseSqlite(sqlite);
        }
    }
    else
    {
        // Use a writable path in containers for SQLite when in production
        string sqlite;
        if (builder.Environment.IsDevelopment())
        {
            sqlite = builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=blog.db";
        }
        else
        {
            var sqlitePath = builder.Configuration["Sqlite:Path"];
            if (string.IsNullOrWhiteSpace(sqlitePath)) sqlitePath = "/data/blog.db";
            sqlite = $"Data Source={sqlitePath}";
        }
        options.UseSqlite(sqlite);
    }
});

// Rate limiting (basic)
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

// Dev-only Swagger and root redirect
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// HTTPS redirect only in prod (Render terminates TLS)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(corsPolicy);
app.UseRateLimiter();

// Global error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

// Ensure DB and run migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        if (!app.Environment.IsDevelopment())
        {
            // Ensure writable directory for SQLite default path
            var dataDir = "/data";
            try { Directory.CreateDirectory(dataDir); } catch { }
        }
        var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
        db.Database.Migrate();
    }
    catch
    {
        // Swallow; middleware will handle runtime errors.
    }
}

// Map root in prod to avoid 404
if (!app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Json(new { service = "Blog API", status = "ok" }));
}

app.Run();