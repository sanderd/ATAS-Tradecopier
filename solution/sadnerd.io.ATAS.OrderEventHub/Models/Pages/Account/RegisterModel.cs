using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using sadnerd.io.ATAS.OrderEventHub.Services;
using Microsoft.AspNetCore.Authorization;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IAccountService _accountService;

        public RegisterModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool IsFirstUser { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";

            [Display(Name = "First Name")]
            public string? FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string? LastName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Only allow registration if no users exist
            IsFirstUser = !await _accountService.HasUsersAsync();
            
            if (!IsFirstUser)
            {
                return RedirectToPage("./Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IsFirstUser = !await _accountService.HasUsersAsync();
            
            if (!IsFirstUser)
            {
                return RedirectToPage("./Login");
            }

            if (ModelState.IsValid)
            {
                var result = await _accountService.CreateUserAsync(
                    Input.Username, 
                    Input.Password, 
                    Input.FirstName, 
                    Input.LastName);
                
                if (result.Succeeded)
                {
                    // Sign in the user immediately after registration
                    await _accountService.SignInAsync(Input.Username, Input.Password, false);
                    return RedirectToPage("/Index");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}