using System.ComponentModel.DataAnnotations;

namespace ModelClasses;

public class Category {
	
	[Key]
	public int    Id   { get; set; }
	
	[StringLength(30, ErrorMessage = "Length of above 30 characters is not allowed")]
	[Required(ErrorMessage = "Name is required")]
	public required string? Name { get; set; }
}
