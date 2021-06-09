using Microsoft.AspNetCore.Builder;

namespace GeneralInsurance.Api.Middleware
{
    public static class RequestResponseLoggingExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogRequestLogging>();
        }

        public static IApplicationBuilder UseCustomHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseHeaders>();
        }
    }
}