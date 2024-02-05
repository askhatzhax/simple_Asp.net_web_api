using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Tasks;
var builder = WebApplication.CreateBuilder();
var app = builder.Build();


app.Run(async (context) =>
{

    var path = context.Request.Path;
    var response = context.Response;
    var request = context.Request;
    context.Response.ContentType = "text/html; charset=utf-8";
    if (path == "/form")
    {
        var message = "Некорректные данные";
        try
        {
            
            var user = await request.ReadFromJsonAsync<UserData>();
            if (user != null)
                message = $"Name: {user.Name} Last name: {user.Last_name}";
        }
        catch { }
        await response.WriteAsJsonAsync(new { text = message });
    }
    else if(path == "/index")
    {
        await context.Response.SendFileAsync("html/form.html");
    }
    else if (path == "/look_image")
        await context.Response.SendFileAsync("image.jpg");
    else if (path == "/download_image")
    {
        context.Response.Headers.ContentDisposition = "attachment; filename=image.jpg";
        await context.Response.SendFileAsync("image.jpg");
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Error 404");
    }
      
});

app.Run();
public class UserData
{
    public string Name { get; set; }
    public string Last_name { get; set; }
}
