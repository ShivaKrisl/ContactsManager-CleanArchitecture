using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters.AuthorizationFilters
{
    public class TokenAuthFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // validate the user
            if (!context.HttpContext.Request.Cookies.ContainsKey("X-Auth-Key"))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            if (context.HttpContext.Request.Cookies["X-Auth-Key"] != "A100")
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            return;
        }
    }
}
