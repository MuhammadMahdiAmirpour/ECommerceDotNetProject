using DatabaseAccess; // Ensure this is the correct namespace for your ApplicationDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ModelClasses;
using MyECommerce.Configuration;
using MyECommerce.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure Entity Framework and Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
		options.SignIn.RequireConfirmedAccount = true;
		options.SignIn.RequireConfirmedEmail   = true;
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<ApplicationUser>, ApplicationUserManager>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Add Razor Pages and MVC
builder.Services.AddRazorPages();
// builder.Services.AddControllersWithViews();

builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(3));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseMigrationsEndPoint();
	app.UseDeveloperExceptionPage(); // Use Developer Exception Page in Development
} else {
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts(); // Use HSTS in Production
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure authentication is used
app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) => {
	try {
		await next();
	} catch (Exception ex) {
		// Log the error
		var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Unhandled exception");

		// Re-throw the exception to be handled by the exception handler middleware
		throw;
	}
});

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
	var logger = app.Services.GetRequiredService<ILogger<Program>>();
	logger.LogCritical(eventArgs.ExceptionObject as Exception, "Unhandled exception");
};

app.Run();
