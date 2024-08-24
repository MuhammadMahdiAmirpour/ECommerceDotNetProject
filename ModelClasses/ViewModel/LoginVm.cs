using System.ComponentModel.DataAnnotations;

namespace ModelClasses.ViewModel;

public class LoginVm {
	[Required(ErrorMessage = "The Email field is required.")]
	[DataType(DataType.EmailAddress)]
	public string? Email { get; set; }

	[Required(ErrorMessage = "The Password filed is required.")]
	[DataType(DataType.Password)]
	public string? Password { get; set; }

	public string? LoginStatus { get; set; }
}
