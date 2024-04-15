using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1;

var builder = WebApplication.CreateBuilder();
string connection = "Server=(localdb)\\mssqllocaldb;Database=applicationdb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
// аутентификация с помощью куки
app.UseAuthentication();   // добавление middleware аутентификации 
app.UseAuthorization();   // добавление middleware авторизации 
app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    // html-форма для ввода логина/пароля
    string loginForm = @"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>METANIT.COM</title>
    </head>
    <body>
        <h2>Login Form</h2>
        <form method='post'>
            <p>
                <label>Email</label><br />
                <input name='email' />
            </p>
            <p>
                <label>Password</label><br />
                <input type='password' name='password' />
            </p>
            <input type='submit' value='Login' />
        </form>
    </body>
    </html>";
    await context.Response.WriteAsync(loginForm);
});

app.MapGet("/", async (HttpContext context) =>
{
    // Redirect to login page
    context.Response.Redirect("/login");
});
app.MapGet("/index", async (HttpContext context) =>
{
    // Получаем путь к файлу index.html
    string indexPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

    // Читаем содержимое файла index.html
    string content = await File.ReadAllTextAsync(indexPath);

    // Возвращаем содержимое файла index.html
    return Results.Ok(content);
});
app.MapGet("/api/users", async (ApplicationContext db) => await db.Users.ToListAsync());

app.MapGet("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользователя по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });

    // если пользователь найден, отправляем его
    return Results.Json(user);
});

app.MapPost("/login", async (string? returnUrl, HttpContext context, ApplicationContext db) =>
{
    // получаем из формы email и пароль
    var form = context.Request.Form;
    // если email и/или пароль не установлены, посылаем статусный код ошибки 400
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email и/или пароль не установлены");

    string email = form["email"];
    string password = form["password"];

    // находим пользователя 
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password==password);
    // если пользователь не найден, отправляем статусный код 401
    //if (user is null) return Results.Unauthorized();
    if (user is null) return Results.BadRequest("Email и/или пароль не правильный");
    if (user is null) return Results.Unauthorized();
    var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Email) };
    // создаем объект ClaimsIdentity
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    // установка аутентификационных куки
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    string indexPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

    // Читаем содержимое файла index.html
    string content = await File.ReadAllTextAsync(indexPath);

    // Возвращаем содержимое файла index.html
    return Results.Content(content, "text/html");
});

app.Run();


