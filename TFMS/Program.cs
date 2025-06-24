using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;
using static QuestPDF.Settings;
using QuestPDF.Infrastructure;


//for app building
var builder = WebApplication.CreateBuilder(args);


//for setting the lincense for questpdf

QuestPDF.Settings.License = LicenseType.Community;
// Add services to the container.
//connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));//registering appDbcontext so sql server is used

builder.Services.AddDatabaseDeveloperPageExceptionFilter();//used for exception handling duringdevelopment


//identity & authorization as  we are proviidng role based authorization
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//registering MVC controller and view support

builder.Services.AddControllersWithViews();
// Program.cs


// Register our services here using DI
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IFuelService, FuelService>(); // Add this line// Add this line
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>(); // Add this line
builder.Services.AddScoped<IPerformanceService, PerformanceService>(); // Add this line


var app = builder.Build();

// Configure the HTTP request pipeline.
//migration  error details
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Index}/{id?}");
app.MapRazorPages();


// database seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
//start web server
app.Run();