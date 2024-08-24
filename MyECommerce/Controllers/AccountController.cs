using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ModelClasses;
using ModelClasses.ViewModel;

namespace MyECommerce.Controllers;

public class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger, IEmailSender emailSender) : Controller {
	// GET
	public IActionResult Index() => View();

	[HttpGet]
	public IActionResult Login() {
		var vm = new LoginVm();
		return View(vm);
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginVm login) {
		if (!ModelState.IsValid) {
			var errors = ModelState.Values.SelectMany(v => v.Errors);
			foreach (var error in errors)
				logger.LogWarning("ValidationError: {ErrorMessage}", error.ErrorMessage);
			return View(login);
		}

		if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password)) {
			login.LoginStatus = "Email and password are required.";
			return View(login);
		}

		login.Email    = login.Email.Trim();
		login.Password = login.Password.Trim();

		logger.LogInformation("Attempt to log in user {Email}", login.Email);

		var user = await userManager.FindByEmailAsync(login.Email);
		if (user == null) {
			logger.LogWarning("User not found: {Email}", login.Email);
			login.LoginStatus = "Invalid email or password.";
			return View(login);
		}

		// Check if the email is confirmed
		if (!await userManager.IsEmailConfirmedAsync(user)) {
			logger.LogWarning("Email not confirmed for user: {Email}", login.Email);
			// Redirect to the email confirmation page
			return RedirectToAction("ResendConfirmationEmail", "Account", new { email = login.Email });
		}

		if (user.UserName == null) return View(login);
		var result = await signInManager.PasswordSignInAsync(user.UserName, login.Password, false, true);

		if (result.Succeeded) {
			logger.LogInformation("User {Email} logged in successfully", login.Email);
			return RedirectToAction("Index", "Home");
		}

		if (result.IsLockedOut) {
			logger.LogWarning("User {Email} is locked out.", login.Email);
			login.LoginStatus = "Account is locked out. Please try again later.";
		} else if (result.IsNotAllowed) {
			logger.LogWarning("User {Email} is not allowed to log in.", login.Email);
			login.LoginStatus = "Login is not allowed for this account.";
		} else if (result.RequiresTwoFactor) {
			logger.LogInformation("User {Email} requires two-factor authentication.", login.Email);
			return RedirectToAction("LoginWith2fa", new { RememberMe = false });
		} else {
			logger.LogWarning("Invalid login attempt for user {Email}.", login.Email);
			login.LoginStatus = "Invalid email or password.";
		}

		return View(login);
	}


	[HttpGet]
	public IActionResult Register() {
		var vm = new RegisterVm();
		return View(vm);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(RegisterVm register) {
		if (!ModelState.IsValid) {
			logger.LogInformation("RegisterVm: {@RegisterVm}", register);
			var errors = ModelState.Values.SelectMany(v => v.Errors);
			foreach (var error in errors) logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
			return View(register);
		}

		var user = new ApplicationUser {
			FirstName             = register.ApplicationUser!.FirstName,
			LastName              = register.ApplicationUser.LastName,
			Email                 = register.Email,
			UserName              = register.UserName,
			Address               = register.ApplicationUser.Address,
			State                 = register.ApplicationUser.State,
			City                  = register.ApplicationUser.City,
			County                = register.ApplicationUser.County,
			PostalCode            = register.ApplicationUser.PostalCode,
			EmailConfirmationDate = null
		};

		if (register.Password != null) {
			var result = await userManager.CreateAsync(user, register.Password);
			if (result.Succeeded) {
				logger.LogInformation("User created a new account with password.");

				var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
				var callbackUrl = Url.Action(
				"ConfirmEmail",
				"Account",
				new { userId = user.Id, code = code },
				protocol: HttpContext.Request.Scheme);

				if (user.Email != null)
					if (callbackUrl != null)
						await emailSender.SendEmailAsync(user.Email, "Confirm your email",
						$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

				register.StatusMessage = "Registration successful. Please check your email to confirm your account.";
				return View("RegisterConfirmation", register);
			}

			foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
		}

		register.StatusMessage = "Registration failed. Please correct the errors and try again.";
		return View(register);
	}

	// [HttpGet]
	// [AllowAnonymous]
	// public async Task<IActionResult> ConfirmEmail(string? userId, string? code) {
	// 	if (userId == null || code == null) return RedirectToAction("Index", "Home");
	// 	var user = await userManager.FindByIdAsync(userId);
	// 	if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");
	// 	var result = await userManager.ConfirmEmailAsync(user, code);
	// 	if (!result.Succeeded) throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
	// 	return View("ConfirmEmail");
	// }


	[HttpGet]
	public async Task<IActionResult> ConfirmEmail(string? userId, string? code) {
		if (userId == null || code == null) return RedirectToAction("Index", "Home");
		var user = await userManager.FindByIdAsync(userId);
		if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");
		var result = await userManager.ConfirmEmailAsync(user, code);
		if (!result.Succeeded) return View("Error");
		user.EmailConfirmationDate = DateTime.UtcNow; // Set the confirmation date
		await userManager.UpdateAsync(user);          // Update the user in the database
		return View("ConfirmEmail");
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> ChangeEmail() {
		var user = await userManager.GetUserAsync(User);
		if (user == null) {
			return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
		}

		var email = await userManager.GetEmailAsync(user);
		return View(new ChangeEmailViewModel { Email = email });
	}

	[HttpPost]
	[Authorize]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model) {
		if (!ModelState.IsValid) return View(model);

		var user = await userManager.GetUserAsync(User);
		if (user == null) return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

		var email = await userManager.GetEmailAsync(user);
		if (model.NewEmail != email) {
			var userId = await userManager.GetUserIdAsync(user);
			var code   = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
			var callbackUrl = Url.Action(
			"ConfirmEmailChange",
			"Account",
			new { userId = userId, email = model.NewEmail, code = code },
			protocol: HttpContext.Request.Scheme);
			if (callbackUrl != null)
				await emailSender.SendEmailAsync(
				model.NewEmail,
				"Confirm your email",
				$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

			return RedirectToAction(nameof(EmailChangeConfirmation));
		}

		ModelState.AddModelError(string.Empty, "New email is the same as your current email.");
		return View(model);
	}

	[HttpGet]
	public IActionResult EmailChangeConfirmation() => View();

	[HttpGet]
	[AllowAnonymous]
	public async Task<IActionResult> ConfirmEmailChange(string? userId, string? email, string? code) {
		if (userId == null || email == null || code == null) return RedirectToAction("Index", "Home");

		var user = await userManager.FindByIdAsync(userId);
		if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

		var result = await userManager.ChangeEmailAsync(user, email, code);
		if (!result.Succeeded) {
			ModelState.AddModelError(string.Empty, "Error changing email.");
			return View();
		}

		// Update the user's username to match the new email
		await userManager.SetUserNameAsync(user, email);

		await signInManager.RefreshSignInAsync(user);
		return View("ConfirmEmailChange");
	}

	// [HttpPost]
	// [AllowAnonymous]
	// [ValidateAntiForgeryToken]
	// public async Task<IActionResult> ResendEmailConfirmation(string email) {
	// 	if (string.IsNullOrEmpty(email)) return View();
	//
	// 	var user = await userManager.FindByEmailAsync(email);
	// 	if (user == null) {
	// 		ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
	// 		return View();
	// 	}
	//
	// 	var userId = await userManager.GetUserIdAsync(user);
	// 	var code   = await userManager.GenerateEmailConfirmationTokenAsync(user);
	// 	var callbackUrl = Url.Action(
	// 	"ConfirmEmail",
	// 	"Account",
	// 	new { userId = userId, code = code },
	// 	protocol: HttpContext.Request.Scheme);
	// 	if (callbackUrl != null)
	// 		await emailSender.SendEmailAsync(
	// 		email,
	// 		"Confirm your email",
	// 		$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
	//
	// 	ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
	// 	return View();
	// }


	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Logout(string? returnUrl = null) {
		await signInManager.SignOutAsync();
		if (returnUrl != null) {
			return LocalRedirect(returnUrl);
		}
		return RedirectToAction("Index", "Home");
	}
	public IActionResult ResendConfirmationEmail(string email) =>
		// Pass the email to the view model
		View(new ResendConfirmationEmailVm { Email = email });

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailVm model) {
		if (!ModelState.IsValid) return View(model);

		var user = await userManager.FindByEmailAsync(model.Email);
		if (user == null) {
			logger.LogWarning("Attempted to resend confirmation email to non-existent user: {Email}", model.Email);
			return RedirectToAction("Login"); // Redirect to login if user does not exist
		}

		var code        = await userManager.GenerateEmailConfirmationTokenAsync(user);
		var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
		await emailSender.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

		logger.LogInformation("Resent confirmation email to user: {Email}", model.Email);
		return RedirectToAction("Login", new { Message = "Confirmation email resent. Please check your inbox." });
	}
}
