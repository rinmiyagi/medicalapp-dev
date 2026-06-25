using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using medicalapp.Areas.Identity.Data;
using medicalapp.Data;
using medicalapp.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("medicalappContextConnection") ?? throw new InvalidOperationException("Connection string 'medicalappContextConnection' not found.");

// local database
builder.Services.AddDbContext<medicalappContext>(options => options.UseSqlServer(connectionString));

// cloud database
// builder.Services.AddDbContext<medicalappContext>(options => options.UseNpgsql(connectionString));


builder.Services.AddDefaultIdentity<medicalappUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<medicalappContext>();
// The scaffolder sets RequireConfirmedAccount = true by default. Change it to false during development.
// Otherwise, users cannot log in after registering until they confirm their email  and no email sender is configured by default.

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); //identity default model

// Register custom services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

app.UseMiddleware<medicalapp.Middleware.GlobalExceptionMiddleware>();

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
app.MapRazorPages(); // identity default model

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await medicalapp.Data.DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

app.Run();
