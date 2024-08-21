using DatabaseAccess;
using Microsoft.AspNetCore.Mvc;

namespace MyECommerce.Controllers;

public class CategoryController(ApplicationDbContext context) : Controller {

	private readonly ApplicationDbContext _context = context;

	public IActionResult Index() {
		var items = _context.Categories.ToList();
		return View();
	}
}
