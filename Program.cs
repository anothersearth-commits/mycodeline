using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using EOM.Web.Data;
using EOM.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Console.WriteLine($"🔌 Connecting to Oracle database: {connectionString.Split(';')[0]}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Simple authentication for development (will be replaced with AD later)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddAuthorization();

// Register AI services
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IAiMessageService, AiMessageService>();

// Configure file upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("🌱 Starting database seeding...");
        await EOM.Web.Data.SeedData.InitializeAsync(services);
        Console.WriteLine("✅ Database seeding completed successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        Console.WriteLine($"❌ Seeding failed: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Inner: {ex.InnerException.Message}");
        }
        
        // Exit after seeding to prevent web server startup issues
        return;
    }
}

Console.WriteLine("🚀 Starting web server...");
app.Run();
