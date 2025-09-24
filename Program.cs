using HRMApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// 1️⃣ Services
// --------------------

// Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core DbContext (PostgreSQL)
// ⚡ Yêu cầu: cài NuGet package Npgsql.EntityFrameworkCore.PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --------------------
// 2️⃣ Configure Kestrel để nhận tất cả IP + PORT Render
// --------------------
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

// --------------------
// 3️⃣ Apply migrations + check DB connection
// --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (!db.Database.CanConnect())
        {
            Console.WriteLine("❌ Cannot connect to database. Check your connection string!");
        }
        else
        {
            Console.WriteLine("✅ Connected to database successfully!");
            db.Database.Migrate(); // Apply migrations tự động
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
    }
}

// --------------------
// 4️⃣ Middleware
// --------------------
app.UseStaticFiles();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// --------------------
// 5️⃣ Run
// --------------------
app.Run();
