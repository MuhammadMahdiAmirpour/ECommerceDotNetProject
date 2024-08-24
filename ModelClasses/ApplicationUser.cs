using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace ModelClasses;

public class ApplicationUser : IdentityUser {
	[Required]
	[Display(Name = "First Name")]
	public string? FirstName { get; set; }

	[Required]
	[Display(Name = "Last Name")]
	public string? LastName { get; set; }

	[Required]
	public string? Address { get; set; }

	[Required]
	public string? City { get; set; }

	[Required]
	public string? County { get; set; }

	[Required]
	public string? PostalCode { get; set; }

	[Required]
	public string? State { get; set; }

	public DateTime? EmailConfirmationDate { get; set; }
}
