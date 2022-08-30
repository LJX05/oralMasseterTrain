namespace aspnetapp.Middleware
{
    public class AuthorityMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cultureQuery = context.Request.Query["culture"];
            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
}
