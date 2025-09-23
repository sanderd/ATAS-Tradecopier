using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sadnerd.io.ATAS.OrderEventHub.Services;

namespace sadnerd.io.ATAS.OrderEventHub.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }
    }
}