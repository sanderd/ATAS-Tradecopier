using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Services;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAccountService _accountService;

        public LogoutModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            await _accountService.SignOutAsync();
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("./Login");
            }
        }
    }
}