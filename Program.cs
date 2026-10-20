var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello from StreakHobby server!");

app.MapGet("/test", () => new
{
    message = "החיבור עובד!",
    success = true
});

app.Run();
