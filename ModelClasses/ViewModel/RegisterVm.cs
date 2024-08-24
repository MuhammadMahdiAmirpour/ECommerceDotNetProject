using System.ComponentModel.DataAnnotations;

namespace ModelClasses.ViewModel;

public class RegisterVm {
	[Required(ErrorMessage = "Application User is required")]
	public ApplicationUser? ApplicationUser { get; set; }

	// [Required(ErrorMessage = "Status message is required")]
	public string? StatusMessage { get; set; } = string.Empty;

	[Required(ErrorMessage = "Email is required")]
	[DataType(DataType.EmailAddress)]
	[EmailAddress(ErrorMessage = "Invalid Email Address")]
	public string? Email { get; set; }

	[Required(ErrorMessage = "Username is required")]
	public string? UserName { get; set; }

	[Required(ErrorMessage = "Password is required")]
	[DataType(DataType.Password)]
	public string? Password { get; set; }

	[Required]
	[DataType(DataType.Password)]
	[Compare("Password", ErrorMessage = "Does not match with the provided password")]
	[Display(Name = "Confirm Password")]
	public string? ConfirmPassword { get; set; }
}
