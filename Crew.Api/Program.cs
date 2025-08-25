var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin() // 或者指定你模拟器的IP:port
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    // 允许外部访问，让Android模拟器访问
    options.ListenAnyIP(5096);
    options.ListenAnyIP(7293, listenOptions =>
    {
        listenOptions.UseHttps(); // 默认开发证书
    }); 
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 启用 Swagger & Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "7293";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
