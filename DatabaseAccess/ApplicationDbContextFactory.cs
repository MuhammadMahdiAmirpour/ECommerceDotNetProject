using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DatabaseAccess;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext> {
	public ApplicationDbContext CreateDbContext(string[] args = null) {
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json") // Ensure this path is correct
			.Build();

		var builder          = new DbContextOptionsBuilder<ApplicationDbContext>();
		var connectionString = configuration.GetConnectionString("DefaultConnection");
		builder.UseSqlServer(connectionString);

		return new ApplicationDbContext(builder.Options);
	}
}
