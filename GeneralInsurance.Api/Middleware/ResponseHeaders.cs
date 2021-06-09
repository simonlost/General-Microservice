using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace GeneralInsurance.Api.Middleware
{
    public class ResponseHeaders
    {
        private readonly RequestDelegate _next;

        public ResponseHeaders(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext) state; httpContext.Response.Headers.Add("App-Semantic-Versions", new[]{Program.AppVersion});
                return Task.FromResult(0);
            },context);
            await _next(context);
        }
    }
}