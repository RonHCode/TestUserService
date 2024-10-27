namespace RazorERPUserService.MiddleWare
{
    public class ThrottlingMiddleware
    {
        private static readonly Dictionary<string, List<DateTime>> _requestLog = new();
        private readonly RequestDelegate _next;

        public ThrottlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.Identity?.Name;
            if (userId != null)
            {
                if (!_requestLog.ContainsKey(userId))
                {
                    _requestLog[userId] = new List<DateTime>();
                }

                var requests = _requestLog[userId];
                requests.Add(DateTime.UtcNow);
                requests.RemoveAll(r => r < DateTime.UtcNow.AddMinutes(-1));

                if (requests.Count > 10)
                {
                    context.Response.StatusCode = 429; // HTTP 429 Too Many Requests 
                    await context.Response.WriteAsync("Too many requests. Try again later.");
                    return;
                }
            }

            await _next(context);
        }
    }

}
