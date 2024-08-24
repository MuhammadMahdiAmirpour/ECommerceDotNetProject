using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ModelClasses;

namespace MyECommerce.Services;

public class ApplicationUserManager(
	IUserStore<ApplicationUser>                      store,
	IOptions<IdentityOptions>                        optionsAccessor,
	IPasswordHasher<ApplicationUser>                 passwordHasher,
	IEnumerable<IUserValidator<ApplicationUser>>     userValidators,
	IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
	ILookupNormalizer                                keyNormalizer,
	IdentityErrorDescriber                           errors,
	IServiceProvider                                 services,
	ILogger<UserManager<ApplicationUser>>            logger)
	: UserManager<ApplicationUser>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
	public override async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token) {
		var result = await base.ConfirmEmailAsync(user, token);
		if (!result.Succeeded) return result;
		user.EmailConfirmationDate = DateTime.UtcNow;
		await UpdateAsync(user);
		return result;
	}
}
