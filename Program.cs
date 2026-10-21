
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// חיבור למסד הנתונים SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=database.db"));

var app = builder.Build();

app.MapGet("/", () => "Hello from StreakHobby server!");

app.MapGet("/test", () => new
{
    message = "החיבור עובד!",
    success = true
});

app.Run();
