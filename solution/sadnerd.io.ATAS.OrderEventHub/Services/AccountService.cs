using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using sadnerd.io.ATAS.OrderEventHub.Identity;

namespace sadnerd.io.ATAS.OrderEventHub.Services
{
    public interface IAccountService
    {
        Task<bool> HasUsersAsync();
        Task<IdentityResult> CreateUserAsync(string username, string password, string? firstName = null, string? lastName = null);
        Task<SignInResult> SignInAsync(string username, string password, bool rememberMe = false);
        Task SignOutAsync();
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user);
    }

    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<bool> HasUsersAsync()
        {
            return await Task.FromResult(_userManager.Users.Any());
        }

        public async Task<IdentityResult> CreateUserAsync(string username, string password, string? firstName = null, string? lastName = null)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = username, // Use username as email for simplicity
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            
            if (result.Succeeded)
            {
                // Ensure default roles exist
                await EnsureRolesExistAsync();
                
                // Assign Admin role to first user
                var userCount = _userManager.Users.Count();
                if (userCount == 1)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
            }

            return result;
        }

        public async Task<SignInResult> SignInAsync(string username, string password, bool rememberMe = false)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, rememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
            }

            return result;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }

        private async Task EnsureRolesExistAsync()
        {
            var roles = new[] { "Admin", "User" };
            
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole 
                    { 
                        Name = roleName,
                        Description = roleName == "Admin" ? "Administrator with full access" : "Standard user"
                    });
                }
            }
        }
    }
}