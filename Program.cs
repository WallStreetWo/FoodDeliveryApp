using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adding services for session state. The shopping cart depends on this.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // The session cookie will be valid for 20 minutes of inactivity.
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register HttpContextAccessor. This allows our service to access the session.
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Register our new ShoppingCartService.
// 'AddScoped' means a new instance of the service is created for each web request.
builder.Services.AddScoped<FoodDeliveryApp.Services.ShoppingCartService>();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Core Identity with ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Sign In Settings
    // This requires users to confirm their email before they can log in.
    // Great for preventing fake accounts. We'll set it to 'true'.
    options.SignIn.RequireConfirmedAccount = true; 

    // Password Strength Settings - Let's enforce strong passwords
    // This directly implements your "Password Strength Meter" requirement from a backend perspective.
    options.Password.RequireDigit = true;           // Must have a number (0-9)
    options.Password.RequireLowercase = true;       // Must have a lowercase letter (a-z)
    options.Password.RequireUppercase = true;       // Must have an uppercase letter (A-Z)
    options.Password.RequireNonAlphanumeric = true; // Must have a special character (e.g., @, #, !)
    options.Password.RequiredLength = 8;            // Must be at least 8 characters long
    options.Password.RequiredUniqueChars = 1;       // At least 1 unique character

    // C. User Lockout Settings - This implements "Rate Limiting / Login Attempt Throttling"
    // Prevents brute-force attacks by locking an account after too many failed attempts.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lock account for 5 minutes
    options.Lockout.MaxFailedAccessAttempts = 5;                      // Lock out after 5 failed login attempts
    options.Lockout.AllowedForNewUsers = true;

    // D. User Settings - Basic rules for usernames/emails
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Ensures every user has a unique email address
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Social Login - Let's add Google as an example
// This requires a NuGet package: Microsoft.AspNetCore.Authentication.Google
// NOTE: You will need to get a Client ID and Client Secret from the Google Cloud Console.
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // These settings are read from your appsettings.json or User Secrets
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

// Add MVC support (controllers + views + razor pages for Identity UI)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Required for Identity scaffolded UI

// Swagger (from your template)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
//app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles(); // Required for serving Identity UI and CSS
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map routes for MVC and Identity UI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Required for Identity UI pages (login, register, etc.)

app.Run();
