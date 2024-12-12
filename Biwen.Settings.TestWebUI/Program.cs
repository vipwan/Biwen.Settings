using Biwen.Settings;
using Biwen.Settings.Extensions.Configuration;
using Biwen.Settings.Caching.Garnet;
using Biwen.Settings.Encryption;
using Biwen.Settings.TestWebUI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpLogging;


Console.WriteLine($"Biwen.Settings Version:{Biwen.Settings.Generated.AssemblyMetadata.Version}");

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

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.CombineLogs = true;
});


//配置garnet client
builder.Services.Configure<GarnetClientOptions>(options =>
{
});

builder.Services.AddBiwenSettings((Action<SettingOptions>)(options =>
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
    options.PermissionValidator = (ctx) => true;
    options.EditorOptions.EditorOnclick = "return confirm('Are You Sure!?');";
    options.EditorOptions.EdtiorConfirmButtonText = "Submit";
    options.EditorOptions.EditorEditButtonText = "Edit";
    options.EditorOptions.ShouldPagenation = true;
    //开启AutoFluentValidation
    options.AutoFluentValidationOption.Enable = true;

    //支持缓存提供者,默认不使用缓存
    //您也可以使用Biwen.Settings提供内存缓存:Biwen.Settings.Caching.MemoryCacheProvider
    //options.UseCacheOfNull();
    options.UseCacheOfMemory();
    //options.UseCacheOfGarnet();

    //加密提供者,空加密为默认实现
    options.UseEncryption<EmptyEncryptionProvider>();


    //必须,否则将初始化错误!
    //使用EFCoreStore
    options.UseStoreOfEFCore(options =>
    {
        //options.DbContextType = typeof(MyDbContext);
        options.UseDbContext<MyDbContext>();
        options.EncryptionOptions = new SettingOptions.EncryptionOptions
        {
            //默认不开启加密
            Enable = true
        };
    });


    //集群的通知服务配置
    options.NotifyOptions.IsNotifyEnable = true;
    options.NotifyOptions.Secret = "Biwen.Settings.Notify";
    options.NotifyOptions.EndpointHosts =
    [
        "http://localhost:5150"
    ];

    //使用JsonStore
    //options.UseStoreOfJsonFile(options =>
    //{
    //    options.FormatJson = true;
    //    options.JsonPath = "systemsettings.json";
    //});

}));

//提供对IConfiguration,IOptions的支持
builder.Configuration.AddBiwenSettingConfiguration(builder.Services, true);

//支持缓存提供者,默认不使用缓存
//您也可以使用Biwen.Settings提供内存缓存:Biwen.Settings.Caching.MemoryCacheProvider
//builder.Services.AddScoped<ICacheProvider, MemoryCacheProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}


app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.MapRazorPages();

app.UseBiwenSettings(mapNotifyEndpoint: true, builder: builder =>
{
    builder.WithTags("BiwenSettingApi").WithOpenApi();
});

app.Run();


namespace Biwen.Settings.TestWebUI
{
    public partial class Program { }
}
