using System.Diagnostics;
using System.Security.Claims;
using Crew.Api.Hubs;
using Crew.Api.Messaging;
using Crew.Api.Middleware;
using Crew.Api.Swagger;
using Crew.Application.Abstractions;
using Crew.Application.Auth;
using Crew.Application.Chat;
using Crew.Application.Events;
using Crew.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Authentication;
using Crew.Api.Authentication;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter(renderMessage: true));
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["traceId"] = traceId;
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSignalR();
builder.Services.AddValidatorsFromAssemblyContaining<GetFeedQueryValidator>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CrewAuthenticationDefaults.SchemeName;
    options.DefaultChallengeScheme = CrewAuthenticationDefaults.SchemeName;
}).AddScheme<AuthenticationSchemeOptions, CrewAuthenticationHandler>(CrewAuthenticationDefaults.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Crew.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                activity.SetTag("db.command", command.CommandText);
            };
        })
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var corsSection = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
allowedOrigins = allowedOrigins.Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
if (allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "https://app.crew.dev" };
}

var allowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>()
    ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
var allowedHeaders = corsSection.GetSection("AllowedHeaders").Get<string[]>()
    ?? new[] { "Authorization", "Content-Type" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(RequestIdMiddleware.CrewAppCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithMethods(allowedMethods)
            .WithHeaders(allowedHeaders);
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"));
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddScoped<UserProvisioningService>();
builder.Services.AddScoped<RegisterForEventCommand>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddSingleton<IOutboxEventHandler, UserJoinedGroupHandler>();

var infrastructureModule = new InfrastructureModule();
infrastructureModule.Install(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseMiddleware<RequestIdMiddleware>();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;

        var statusCode = exception switch
        {
            InvalidOperationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Invalid operation",
                StatusCodes.Status403Forbidden => "Forbidden",
                _ => "An unexpected error occurred"
            },
            Detail = statusCode == StatusCodes.Status500InternalServerError && !app.Environment.IsDevelopment()
                ? "An unexpected error occurred"
                : exception?.Message,
            Instance = context.Request.Path
        };

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        problem.Extensions["traceId"] = traceId;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
        logger.LogError(exception, "Unhandled exception");

        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Crew API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

await app.ApplyMigrationsAsync();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        if (httpContext.Items.TryGetValue(RequestIdMiddleware.RequestIdItemName, out var value) && value is string requestId)
        {
            diagnosticContext.Set("request_id", requestId);
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            diagnosticContext.Set("user_id", userId);
        }
    };
});

app.UseRouting();
app.UseCors(RequestIdMiddleware.CrewAppCorsPolicy);
app.UseMiddleware<FirebaseAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

public partial class Program;
