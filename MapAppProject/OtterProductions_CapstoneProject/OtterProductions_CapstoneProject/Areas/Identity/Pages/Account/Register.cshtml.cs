﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using OtterProductions_CapstoneProject.Data;
using OtterProductions_CapstoneProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OtterProductions_CapstoneProject.Areas.Identity.Data;

namespace OtterProductions_CapstoneProject.Areas.Identity.Pages.Account
{
    
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly MapAppDbContext _mapAppDbContext;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            MapAppDbContext context)      // inject the dbcontext we need to create our own user
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger ;
            _emailSender = emailSender;
            _mapAppDbContext = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Add fields to get further user info
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //var user = CreateUser();
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);
                
                //var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    // Check and see if the id was set in the IdentityUser object when it was successfully created
                    _logger.LogInformation($"User created a new account with password, id is {user.Id}");

                    // Create one of our users
                    MapAppUser map = new MapAppUser
                    {
                        AspnetIdentityId = user.Id,     // get the aspnet id of the newly created identity user
                        FirstName = Input.FirstName,
                        LastName = Input.LastName
                    };
                    _mapAppDbContext.Add(map);
                    await _mapAppDbContext.SaveChangesAsync();

                    //var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}







//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//#nullable disable

//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Text.Encodings.Web;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.WebUtilities;
//using Microsoft.Extensions.Logging;
//using OtterProductions_CapstoneProject.Areas.Identity.Data;
//using OtterProductions_CapstoneProject.Data;
//using OtterProductions_CapstoneProject.Models;

//namespace OtterProductions_CapstoneProject.Areas.Identity.Pages.Account
//{
//    [AllowAnonymous]
//    public class RegisterModel : PageModel
//    {
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly IUserStore<ApplicationUser> _userStore;
//        private readonly IUserEmailStore<ApplicationUser> _emailStore;
//        private readonly ILogger<RegisterModel> _logger;
//        private readonly IEmailSender _emailSender;
//        private readonly MapAppDbContext _mapAppDbContext;

//        public RegisterModel(
//            UserManager<ApplicationUser> userManager,
//            IUserStore<ApplicationUser> userStore,
//            SignInManager<ApplicationUser> signInManager,
//            ILogger<RegisterModel> logger,
//            IEmailSender emailSender,
//            MapAppDbContext context)
//        {
//            _userManager = userManager;
//            _userStore = userStore;
//            _signInManager = signInManager;
//            _logger = logger;
//            _emailSender = emailSender;
//            _mapAppDbContext = context;
//        }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        [BindProperty]
//        public InputModel Input { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public string ReturnUrl { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public IList<AuthenticationScheme> ExternalLogins { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public class InputModel
//        {
//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Required]
//            [EmailAddress]
//            [Display(Name = "Email")]
//            public string Email { get; set; }

//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Required]
//            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
//            [DataType(DataType.Password)]
//            [Display(Name = "Password")]
//            public string Password { get; set; }

//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [DataType(DataType.Password)]
//            [Display(Name = "Confirm password")]
//            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
//            public string ConfirmPassword { get; set; }

//            [Required]
//            [Display(Name = "First Name")]
//            public string FirstName { get; set; }

//            [Required]
//            [Display(Name = "Last Name")]
//            public string LastName { get; set; }
//        }


//        public async Task OnGetAsync(string returnUrl = null)
//        {
//            ReturnUrl = returnUrl;
//            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
//        }

//        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
//        {
//            returnUrl ??= Url.Content("~/");
//            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
//            if (ModelState.IsValid)
//            {
//                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email};

//                //await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
//                //await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
//                var result = await _userManager.CreateAsync(user, Input.Password);

//                if (result.Succeeded)
//                {
//                    _logger.LogInformation("User created a new account with password.");


//                    MapAppUser appUser = new MapAppUser
//                    {
//                        AspnetIdentityId = user.Id,
//                        FirstName = Input.FirstName,
//                        LastName = Input.LastName,

//                    };

//                    _mapAppDbContext.Add(appUser);
//                    await _mapAppDbContext.SaveChangesAsync();

//                    var userId = await _userManager.GetUserIdAsync(user);
//                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
//                    var callbackUrl = Url.Page(
//                        "/Account/ConfirmEmail",
//                        pageHandler: null,
//                        values: new { area = "Identity", /*userId = userId,*/ code = code, returnUrl = returnUrl },
//                        protocol: Request.Scheme);

//                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
//                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

//                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
//                    {
//                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
//                    }
//                    else
//                    {
//                        await _signInManager.SignInAsync(user, isPersistent: false);
//                        return LocalRedirect(returnUrl);
//                    }
//                }
//                foreach (var error in result.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//            }

//            // If we got this far, something failed, redisplay form
//            return Page();
//        }

//        private ApplicationUser CreateUser()
//        {
//            try
//            {
//                return Activator.CreateInstance<ApplicationUser>();
//            }   
//            catch
//            {
//                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
//                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
//                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
//            }
//        }

//        //private IUserEmailStore<ApplicationUser> GetEmailStore()
//        //{
//        //    if (!_userManager.SupportsUserEmail)
//        //    {
//        //        throw new NotSupportedException("The default UI requires a user store with email support.");
//        //    }
//        //    return (IUserEmailStore<ApplicationUser>)_userStore;
//        //}
//    }
//}
