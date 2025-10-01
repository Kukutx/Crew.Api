using Crew.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services
    .AddApplicationOptions(builder.Configuration)
    .AddApplicationDatabase(builder.Configuration)
    .AddApplicationServices()
    .AddApplicationSecurity(builder.Configuration)
    .AddApplicationSwagger()
    .AddApplicationCors();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseAppSwagger();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
