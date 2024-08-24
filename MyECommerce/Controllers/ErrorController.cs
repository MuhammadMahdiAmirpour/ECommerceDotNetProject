using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModelClasses.ViewModel;

namespace MyECommerce.Controllers;

public class ErrorController : Controller {
	public IActionResult Error() {
		var errorVm = new ErrorVm {
			RequestId     = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
			ShowRequestId = !string.IsNullOrEmpty(Activity.Current?.Id ?? HttpContext.TraceIdentifier)
		};
		return View(errorVm);
	}
}
