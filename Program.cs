var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<JaezooServer.Hubs.ChatHub>("/chatHub");

app.Run();