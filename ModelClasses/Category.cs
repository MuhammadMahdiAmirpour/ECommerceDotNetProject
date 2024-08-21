using System.ComponentModel.DataAnnotations;

namespace ModelClasses;

public class Category {
	
	[Key]
	public int    Id   { get; set; }
	
	[Required]
	[StringLength(30, ErrorMessage = "Length of above 30 characters is not allowed")]
	public required string Name { get; set; }
}
