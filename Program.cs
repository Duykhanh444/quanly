using HRMApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// 1️⃣ Services
// --------------------

// Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore reference cycles to avoid JSON serialization errors
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enable CORS (Allow all origins, methods, headers)
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
// 2️⃣ Configure Kestrel with dynamic port
// --------------------
var portEnv = Environment.GetEnvironmentVariable("PORT");
var port = !string.IsNullOrEmpty(portEnv) ? int.Parse(portEnv) : 5216;
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port); // Listen on all IPs
});

var app = builder.Build();

// --------------------
// 3️⃣ Apply migrations on startup
// --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // Apply migrations automatically
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
app.Run($"http://0.0.0.0:{port}");
