using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

// הרשמת משתמש חדש
app.MapPost("/register", async (RegisterRequest request, AppDbContext db) =>
{
    // בדיקה ששם המשתמש והסיסמה אינם ריקים
    if (string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new
        {
            message = "Username and password are required."
        });
    }

    // הסרת רווחים מיותרים בתחילת שם המשתמש ובסופו
    string username = request.Username.Trim();

    // בדיקה אם שם המשתמש כבר קיים במסד הנתונים
    bool userExists = await db.Users
        .AnyAsync(user => user.Username == username);

    if (userExists)
    {
        return Results.Conflict(new
        {
            message = "Username already exists."
        });
    }

    // יצירת משתמש חדש
    User newUser = new User();

    newUser.Username = username;
    newUser.Email = "";

    // הצפנת הסיסמה לפני שמירתה במסד הנתונים
    PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

    newUser.PasswordHash = passwordHasher.HashPassword(
        newUser,
        request.Password
    );

    // הוספת המשתמש למסד הנתונים ושמירת השינויים
    db.Users.Add(newUser);

    await db.SaveChangesAsync();

    // החזרת תשובה שההרשמה הצליחה
    return Results.Ok(new
    {
        message = "Registration successful!",
        success = true
    });
});

app.Run();

// מבנה הנתונים שמתקבלים בבקשת הרשמה
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
