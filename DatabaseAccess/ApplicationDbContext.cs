using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModelClasses;

namespace DatabaseAccess;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
	public DbSet<Category>        Categories       { get; set; }
	public DbSet<ApplicationUser> ApplicationUsers { get; set; }
}
