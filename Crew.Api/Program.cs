using Crew.Api.Extensions;
using Crew.Api.Hubs;
using Crew.Api.Messaging;
using Crew.Api.Middleware;
using Crew.Application.Abstractions;
using Crew.Application.Auth;
using Crew.Application.Chat;
using Crew.Application.Events;
using Crew.Infrastructure.Extensions;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.Services.AddScoped<UserProvisioningService>();
builder.Services.AddScoped<RegisterForEventCommand>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddSingleton<IOutboxEventHandler, UserJoinedGroupHandler>();

var infrastructureModule = new InfrastructureModule();
infrastructureModule.Install(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        PostgresDatabaseInitializer.EnsureDatabase(dbContext, logger);
        dbContext.Database.Migrate();
    }
    catch (Exception exception)
    {
        logger.LogCritical(exception, "An error occurred while initializing the database.");
        throw;
    }
}

app.UseRouting();
app.UseCors();
app.UseMiddleware<FirebaseAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
