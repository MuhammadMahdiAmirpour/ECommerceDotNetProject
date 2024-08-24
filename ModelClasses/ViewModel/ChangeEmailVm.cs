using System.ComponentModel.DataAnnotations;

namespace ModelClasses.ViewModel;

public class ChangeEmailViewModel {
	[Required]
	[EmailAddress]
	[Display(Name = "Current Email")]
	public string? Email { get; set; }

	[Required]
	[EmailAddress]
	[Display(Name = "New Email")]
	public string NewEmail { get; set; }
}
