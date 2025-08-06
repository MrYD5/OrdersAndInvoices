public class RoleRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public RoleRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var user = context.User;
            var path = context.Request.Path.ToString().ToLower();

            // Изключения за Account, статични файлове и POST заявки
            if (path.StartsWith("/account") || path.StartsWith("/identity") || context.Request.Method == "POST")
            {
                await _next(context);
                return;
            }

            // Ако е Check роля и се опитва да достъпи други страници
            if (user.IsInRole("Check") && !(path.StartsWith("/home/workers") || path.StartsWith("/home/submitproducts")))
            {
                context.Response.Redirect("/Home/Workers");
                return;
            }
        }

        await _next(context);
    }
}
