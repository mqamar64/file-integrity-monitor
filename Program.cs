using FileIntegrityMonitor.Data;
using FileIntegrityMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register application services for dependency injection.
// HashDb is a singleton so the same database service is reused.
// ScanService and ComparisonService are scoped per request.
builder.Services.AddSingleton<HashDb>();
builder.Services.AddScoped<ScanService>();
builder.Services.AddScoped<ComparisonService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Default route sends users to the Scans dashboard first.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Scans}/{action=Index}/{id?}");

app.Run();