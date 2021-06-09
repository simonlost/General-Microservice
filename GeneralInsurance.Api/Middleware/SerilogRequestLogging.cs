using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GeneralInsurance.Api.Middleware
{
    public class SerilogRequestLogging
    {
        private const string MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:00000} ms";

        private static readonly ILogger Log = SerilogRequestLogging.Log.ForContext<SerilogRequestLogging>();
        private readonly RequestDelegate _next;

        public SerilogRequestLogging(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if(httpContext ==null) throw new ArgumentNullException(nameof(httpContext));

            var requestUtcDateTime = DateTime.UtcNow;
            var start = Stopwatch.GetTimestamp();
            try
            {
                await _next(httpContext);
                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
                var log = AddHttpRequestParamsAsContext(httpContext);
                log = log.ForContext("RequestUtcDateTime", requestUtcDateTime.ToString());
                log.Write(level, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, statusCode,
                    elapsedMs);
            }
            catch (Exception ex)
            {
                LogException(httpContext, requestUtcDateTime, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()),
                    ex);
            }
        }

        static Ilogger AddHttpRequestParamsAsContext(HttpContext httpContext, bool includeFormData = false)
        {
            var request = httpContext.Request;
            var log = Log
                .ForContext("RequestHeaders",
                    request.Headers
                        .Where(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) == false)
                        .ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                .ForContext("RequestHost", request.Host).ForContext("RequestProtocol", request.Protocol);
            if (request.HasFormContentType && includeFormData)
                log = log.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));
            return log;
        }

        static bool LogException(HttpContext httpContext, DateTime requestUtcDateTime, double elaspsedMs, Exception ex)
        {
            var log = AddHttpRequestParamsAsContext(httpContext, includeFormData: true);
            log = log.ForContext("RequestUtcDateTime", requestUtcDateTime.ToString());
            log.Error(ex, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, 500,
                elaspsedMs);
            return false;
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double) Stopwatch.Frequency;
        }
    }
}