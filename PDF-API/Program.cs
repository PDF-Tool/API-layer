using PDF_API.Adapters;
using PDF_API.Services;
using Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILogger<MessagingService>>(sp =>
    sp.GetRequiredService<ILoggerFactory>().CreateLogger<MessagingService>());
builder.Services.AddSingleton<MessagingService>();
builder.Services.AddTransient<WebSocketAdapter>();
builder.Services.AddScoped<Logic.APIController>();
builder.Services.AddScoped<InputService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("X-Connection-Id"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSockets();

app.UseCors("AllowAnyOrigin");
app.Map("/ws", async (string name, HttpContext context, WebSocketAdapter ws) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        await ws.HandleUser(context, name);
    }
    else
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Expected a WebSocket request");
    }
});


app.MapControllers();

app.Run();
