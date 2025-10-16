using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Crew.Api.Middleware;

public sealed class RequestIdMiddleware
{
    public const string RequestIdItemName = "RequestId";
    public const string CrewAppCorsPolicy = "CrewApp";
    private const string RequestIdHeader = "X-Request-Id";

    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = GetOrCreateRequestId(context);
        context.Items[RequestIdItemName] = requestId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[RequestIdHeader] = requestId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("request_id", requestId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(RequestIdHeader, out StringValues existing) && !StringValues.IsNullOrEmpty(existing))
        {
            return existing.ToString();
        }

        var generated = Guid.NewGuid().ToString();
        context.Request.Headers[RequestIdHeader] = generated;
        return generated;
    }
}
