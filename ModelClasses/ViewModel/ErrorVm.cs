namespace ModelClasses.ViewModel;

public class ErrorVm
{
	public string? RequestId     { get; set; }
	public bool    ShowRequestId { get; set; }

	public string? ExceptionMessage     { get; set; }
	public string? StackTrace           { get; set; }
	public bool    ShowExceptionDetails { get; set; } // Set to true in development

	// Optional: Add a property for custom error messages
	public string? CustomErrorMessage { get; set; }
}
