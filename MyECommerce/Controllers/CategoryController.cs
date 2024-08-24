using DatabaseAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelClasses;

namespace MyECommerce.Controllers {
	public class CategoryController(ApplicationDbContext context) : Controller {
		// GET: Category/Index
		public IActionResult Index() {
			var items = context.Categories.ToList();
			return View(items);
		}

		// GET: Category/Upsert
		[Authorize]
		public IActionResult Upsert(int? id) {
			var category = new Category {
				Name = null
			}; // Create a new category object

			if (id == null) return View(category); // Return the view with a new category

			// If an id is provided, fetch the existing category
			category = context.Categories.Find(id);
			if (category == null) return NotFound(); // Return 404 if category not found

			return View(category); // Return the existing category to the view
		}

		// POST: Category/Upsert
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Upsert(Category category) {
			if (!ModelState.IsValid) return View(category); // Return the same view if model state is invalid

			if (category.Id == 0) {
				// If the category is new
				// if the name is already in the database
				if (await context.Categories.FirstOrDefaultAsync(u => u.Name == category.Name) is not null) {
					TempData["AlertMessage"] = category.Name + " is an existing item found in the list, so not added to the list";
					return RedirectToAction("Index");
				}
				await context.Categories.AddAsync(category);
				TempData["AlertMessage"] = category.Name + " has been added to the list";
			} else {
				// If the category is being updated
				context.Categories.Update(category);
				TempData["AlertMessage"] = category.Name + " has been Edited in the list";
			}

			await context.SaveChangesAsync();       // Save changes to the database
			return RedirectToAction(nameof(Index)); // Redirect to the Index action
		}

		// GET: Category/Delete/id
		public IActionResult Delete(int id) {
			var category = context.Categories.Find(id);
			if (category == null) return NotFound(); // Return 404 if category not found

			return View(category); // Return the category to the view for confirmation
		}

		// POST: Category/Delete/id
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(Category? category) {
			if (category == null) return NotFound(); // Return 404 if category not found

			context.Categories.Remove(category); // Remove the category from the context
			await context.SaveChangesAsync();    // Save changes to the database

			TempData["AlertMessage"] = category.Name + " has been deleted from the category list";

			return RedirectToAction(nameof(Index)); // Redirect to the Index action
		}
	}
}
