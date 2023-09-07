using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Logging;
using Serilog;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, configBuilder) =>
    {
        configBuilder.ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext();
    });



//解决中文被编码
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

builder.Services.Configure<IdentityOptions>(o =>
{
    //配置默认密码规则
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireDigit = false;
    o.Password.RequireLowercase = false;
    o.Password.RequiredUniqueChars = 0;
    o.Password.RequireUppercase = false;
    o.Password.RequireLowercase = false;
});

builder.Services
    .AddOrchardCms(x =>
    {
        //x.EnableFeature("Biwen.Settings.OC");
    })
// // Orchard Specific Pipeline
// .ConfigureServices( services => {
// })
// .Configure( (app, routes, services) => {
// })
;



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());

app.Run();
