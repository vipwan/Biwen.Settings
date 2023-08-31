using Biwen.Settings;
using Biwen.Settings.TestWebUI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



//×¢²áDbContext
builder.Services.AddDbContext<MyDbContext>(options =>
{
    //just for test
    //options.UseInMemoryDatabase("BiwenSettings");
    options.UseSqlite("Data Source=BiwenSettings.db");
});


builder.Services.AddBiwenSettings(typeof(MyDbContext), options =>
{

    //#if DEBUG
    //    options.ProjectId = $"Biwen.Settings.TestWebUI-{"Development"}";
    //#endif

#if DEBUG
    options.ProjectId = $"Biwen.Settings.TestWebUI-{"Production"}";
#endif

    options.Layout = "~/Views/Shared/_Layout.cshtml";
    options.Title = "Biwen.Settings";
    options.Route = "system/settings";
    options.Valider = (ctx) => true;
    options.EditorOption.EditorOnclick = "return confirm('Are You Sure!?');";
    options.EditorOption.EdtiorConfirmButtonText = "Submit";
    options.EditorOption.EditorEditButtonText = "Edit";
    options.EditorOption.ShouldPagenation = true;

}, true);

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
