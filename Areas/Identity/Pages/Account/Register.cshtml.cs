// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;

namespace WebTrackED_CHED_MIMAROPA.Areas.Identity.Pages.Account
{
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IUserStore<AppIdentityUser> _userStore;
        private readonly IUserEmailStore<AppIdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IBaseRepository<AppIdentityUser> _accRepo;
        private readonly EmailSender _emailSender;
        private readonly IBaseRepository<Settings> _settingsRepo;

        public RegisterModel(
            UserManager<AppIdentityUser> userManager,
            IUserStore<AppIdentityUser> userStore,
            SignInManager<AppIdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            EmailSender emailSender,
            IBaseRepository<AppIdentityUser> accRepo,
            IBaseRepository<Settings> settingsRepo)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _accRepo = accRepo;
            _emailSender = emailSender;
            _settingsRepo = settingsRepo;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        public string LogoFileName { get; set; }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public TypeOfUser TypeOfUser { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public bool FirstUser { get; set; }


        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            var settings = await _settingsRepo.GetAll();
            var updatedSettings = settings.OrderByDescending(x => x.Id).First();
            if (!updatedSettings.EnableRegistration)
                return BadRequest("Unable to register");

            LogoFileName = updatedSettings.LogoFileName;
            var users = await _accRepo.GetAll();
            FirstUser = users.Where(x => x.EmailConfirmed == true).Count() <= 0;
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var settings = await _settingsRepo.GetAll();
                var settingsUpdated = settings.OrderByDescending(x => x.Id).First();
                LogoFileName = settingsUpdated.LogoFileName;
                if (Input.Password.Length >= settingsUpdated.PasswordRequiredLength)
                {
					var user = CreateUser();
					var users = await _accRepo.GetAll();
					var typeOfUser = users.Where(x => x.EmailConfirmed == true).Count() <= 0 ? TypeOfUser.Admin : TypeOfUser.Sender;
					user.TypeOfUser = typeOfUser;
					await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
					await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
					var result = await _userManager.CreateAsync(user, Input.Password);

					if (result.Succeeded)
					{
						_logger.LogInformation("User created a new account with password.");
						await _userManager.AddToRoleAsync(user, typeOfUser == TypeOfUser.Admin ? "Admin" : "Sender");

						var userId = await _userManager.GetUserIdAsync(user);
						var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
						code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
						var callbackUrl = Url.Page(
							"/Account/PartialInformation",
							pageHandler: null,
							values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
							protocol: Request.Scheme);

						await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
							$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

						if (_userManager.Options.SignIn.RequireConfirmedAccount)
						{
							//return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl }
							TempData["done-form-pass"] = true;
							return RedirectToPage("./EmailConfirmed");
						}
						else
						{
							await _signInManager.SignInAsync(user, isPersistent: false);
							TempData["done-form-pass"] = true;
							return LocalRedirect(returnUrl);
						}
					}
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
                else
                {
                    ModelState.AddModelError(string.Empty,$"Passwords must be at least {settingsUpdated.PasswordRequiredLength} characters.");
                }
            }

            // If we got this far, something failed, redisplay form
            TempData["done-form-pass"] = true;
            return Page();
        }

        private AppIdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppIdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppIdentityUser)}'. " +
                    $"Ensure that '{nameof(AppIdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private IUserEmailStore<AppIdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppIdentityUser>)_userStore;
        }
    }
}
