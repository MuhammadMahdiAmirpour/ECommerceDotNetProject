using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModelClasses.ViewModel;

namespace MyECommerce.Controllers;

public class ErrorController(IHostEnvironment environment, ILogger<ErrorController> logger) : Controller {
	[Route("/error")]
	public IActionResult Error()
	{
		logger.LogError("Error Page accessed");
		var context   = HttpContext.Features.Get<IExceptionHandlerFeature>();
		var exception = context?.Error;

		var errorVm = new ErrorVm
		{
			RequestId     = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
			ShowRequestId = !string.IsNullOrEmpty(Activity.Current?.Id ?? HttpContext.TraceIdentifier)
		};

		if (environment.IsDevelopment())
		{
			errorVm.ExceptionMessage     = exception?.Message;
			errorVm.StackTrace           = exception?.StackTrace;
			errorVm.ShowExceptionDetails = true;
		}
		else
		{
			errorVm.CustomErrorMessage = "An error occurred. Please try again later.";
		}

		if (exception != null)
		{
			logger.LogError(exception, "An unhandled exception occurred");
		}

		return View(errorVm);
	}

	[Route("/error/404")]
	public IActionResult Error404()
	{
		var errorVm = new ErrorVm
		{
			RequestId          = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
			ShowRequestId      = !string.IsNullOrEmpty(Activity.Current?.Id ?? HttpContext.TraceIdentifier),
			CustomErrorMessage = "The requested resource was not found."
		};

		return View("Error", errorVm);
	}
}
