using Biwen.Settings;
using Biwen.Settings.Encryption;
using Biwen.Settings.TestWebUI.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//注册DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
{
    //just for test
    //options.UseInMemoryDatabase("BiwenSettings");
    options.UseSqlite("Data Source=BiwenSettings.db");
});


builder.Services.AddBiwenSettings(options =>
{

#if DEBUG
    options.ProjectId = $"Biwen.Settings.TestWebUI-{"Development"}";
#endif

#if !DEBUG
    options.ProjectId = $"Biwen.Settings.TestWebUI-{"Production"}";
#endif

    //options.Layout = "~/Views/Shared/_Layout.cshtml";
    options.Title = "Biwen.Settings";
    options.Route = "system/settings";
    options.HasPermission = (ctx) => true;
    options.EditorOption.EditorOnclick = "return confirm('Are You Sure!?');";
    options.EditorOption.EdtiorConfirmButtonText = "Submit";
    options.EditorOption.EditorEditButtonText = "Edit";
    options.EditorOption.ShouldPagenation = true;
    //开启AutoFluentValidation
    options.AutoFluentValidationOption.Enable = true;

    //支持缓存提供者,默认不使用缓存
    //您也可以使用Biwen.Settings提供内存缓存:Biwen.Settings.Caching.MemoryCacheProvider
    //options.UseCacheOfNull();
    options.UseCacheOfMemory();

    //加密提供者,空加密为默认实现
    options.UseEncryption<EmptyEncryptionProvider>();


    //必须,否则将初始化错误!
    //使用EFCoreStore
    options.UseStoreOfEFCore(options =>
    {
        options.DbContextType = typeof(MyDbContext);
        options.EncryptionOption = new SettingOptions.EncryptionOptions
        {
            //默认不开启加密
            Enable = true
        };
    });


    //集群的通知服务配置
    options.NotifyOption.IsNotifyEnable = true;
    options.NotifyOption.Secret = "Biwen.Settings.Notify";
    options.NotifyOption.EndpointHosts = new[]
    {
        "http://localhost:5150"
    };

    //使用JsonStore
    //options.UserStoreOfJsonFile(options =>
    //{
    //    options.FormatJson = true;
    //    options.JsonPath = "1systemsetting.json";
    //});

});


//支持缓存提供者,默认不使用缓存
//您也可以使用Biwen.Settings提供内存缓存:Biwen.Settings.Caching.MemoryCacheProvider
//builder.Services.AddScoped<ICacheProvider, MemoryCacheProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}



app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseBiwenSettings();
//map api
app.MapBiwenSettingApi(mapNotifyEndpoint: true).WithTags("BiwenSettingApi").WithOpenApi();


app.Run();
