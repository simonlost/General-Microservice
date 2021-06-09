using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Serilog.Context;

namespace GeneralInsurance.Api.Middleware
{
    public class MandatoryCorrelationId
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<MandatoryCorrelationId>();
        public static string ManadatoryCorrelationIdHeader => "x-fapi-interaction-id";
        readonly RequestDelegate _next;

        public MandatoryCorrelationId(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string correlationIdHeader = httpContext.Request.Headers(ManadatoryCorrelationIdHeader);
            if (string.IsNullOrWhiteSpace(correlationIdHeader))
            {
                httpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                Log.Warning($"{ManadatoryCorrelationIdHeader} header is missing");
                return;
            }

            if (!Guid.TryParse(correlationIdHeader, out _))
            {
                httpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                Log.Warning($"{ManadatoryCorrelationIdHeader} header is not a GUID");
                return;
            }

            using (LogContext.PushProperty(ManadatoryCorrelationIdHeader, correlationIdHeader))
            {
                await _next.Invoke(httpContext);
            }
        }
    }
}