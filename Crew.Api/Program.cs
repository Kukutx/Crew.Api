using System.Reflection;
using System.Linq;
using Crew.Api.Hubs;
using Crew.Api.Messaging;
using Crew.Api.Middleware;
using Crew.Application.Abstractions;
using Crew.Application.Auth;
using Crew.Application.Chat;
using Crew.Application.Events;
using Microsoft.EntityFrameworkCore;

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

var infrastructureAssembly = Assembly.Load("Crew.Infrastructure");
var installerType = infrastructureAssembly
    .GetTypes()
    .FirstOrDefault(t => typeof(IModuleInstaller).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

if (installerType is not null && Activator.CreateInstance(installerType) is IModuleInstaller installer)
{
    installer.Install(builder.Services, builder.Configuration);
}
else
{
    throw new InvalidOperationException("Infrastructure module could not be loaded.");
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContextType = infrastructureAssembly.GetType("Crew.Infrastructure.Persistence.AppDbContext");
    if (dbContextType is not null)
    {
        if (scope.ServiceProvider.GetService(dbContextType) is DbContext dbContext)
        {
            dbContext.Database.Migrate();
        }
    }
}

app.UseRouting();
app.UseCors();
app.UseMiddleware<FirebaseAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
