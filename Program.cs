
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
    if (string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new
        {
            message = "Username and password are required."
        });
    }

    string username = request.Username.Trim();

    bool userExists = await db.Users
        .AnyAsync(user => user.Username == username);

    if (userExists)
    {
        return Results.Conflict(new
        {
            message = "Username already exists."
        });
    }

    User newUser = new User();

    newUser.Username = username;
    newUser.Email = "";

    // הצפנת הסיסמה
    PasswordHasher<User> passwordHasher =
        new PasswordHasher<User>();

    newUser.PasswordHash = passwordHasher.HashPassword(
        newUser,
        request.Password
    );

    db.Users.Add(newUser);

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "Registration successful!",
        success = true
    });
});

// התחברות של משתמש קיים
app.MapPost("/login", async (LoginRequest request, AppDbContext db) =>
{
    // בדיקה שהמשתמש והסיסמה הוזנו
    if (string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new
        {
            message = "Username and password are required."
        });
    }

    string username = request.Username.Trim();

    // חיפוש המשתמש במסד הנתונים
    User? user = await db.Users
        .FirstOrDefaultAsync(user => user.Username == username);

    // אם המשתמש לא קיים
    if (user == null)
    {
        return Results.Unauthorized();
    }

    // בדיקת הסיסמה מול הסיסמה המוצפנת במסד הנתונים
    PasswordHasher<User> passwordHasher =
        new PasswordHasher<User>();

    PasswordVerificationResult result =
        passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

    // אם הסיסמה שגויה
    if (result == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    // ההתחברות הצליחה
    return Results.Ok(new
    {
        message = "Login successful!",
        success = true,
        username = user.Username
    });
});

app.Run();

// מבנה הנתונים שמתקבלים בבקשת הרשמה
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

// מבנה הנתונים שמתקבלים בבקשת התחברות
public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
