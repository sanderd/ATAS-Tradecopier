using sadnerd.io.ATAS.OrderEventHub.Services;

namespace sadnerd.io.ATAS.OrderEventHub.Middleware
{
    public class FirstTimeSetupMiddleware
    {
        private readonly RequestDelegate _next;

        public FirstTimeSetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAccountService accountService)
        {
            // Skip middleware for specific paths to avoid redirect loops
            var path = context.Request.Path.Value?.ToLower();
            var skipPaths = new[] { "/account/register", "/account/login", "/account/logout", "/_content", "/lib", "/css", "/js", "/favicon.ico" };
            
            if (skipPaths.Any(skipPath => path?.StartsWith(skipPath) == true))
            {
                await _next(context);
                return;
            }

            // Check if any users exist in the system
            var hasUsers = await accountService.HasUsersAsync();
            
            if (!hasUsers && path != "/account/register")
            {
                // Redirect to registration page if no users exist
                context.Response.Redirect("/Account/Register");
                return;
            }

            // If users exist but user is not authenticated (except for login page), redirect to login
            if (hasUsers && !context.User.Identity.IsAuthenticated && path != "/account/login")
            {
                // Store the original URL to redirect back after login
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }

            await _next(context);
        }
    }

    public static class FirstTimeSetupMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirstTimeSetup(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirstTimeSetupMiddleware>();
        }
    }
}