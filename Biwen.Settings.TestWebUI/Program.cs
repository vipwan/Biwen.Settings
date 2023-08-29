using Biwen.Settings;
using Biwen.Settings.TestWebUI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



//×¢²áDbContext
builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseInMemoryDatabase("BiwenSettings");
});


builder.Services.AddBiwenSettings(typeof(MyDbContext), options =>
{
    options.Layout = "~/Views/Shared/_Layout.cshtml";
    options.Title = "Biwen.Settings";
    options.Route = "system/settings";
    options.Valider = (context) =>
    {
        return true;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();


app.UseBiwenSettings();


app.Run();
