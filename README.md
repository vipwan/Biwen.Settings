# Biwen.Settings

![image](https://github.com/vipwan/Biwen.Settings/assets/13956765/b438393b-f5d9-4d78-b2aa-c20851fb9880)




## NuGet 包

- dotnet add package Biwen.Settings --version 1.0.1

## 开发环境

* Windows 10
* [Rider 2022](https://www.jetbrains.com/rider) / [Visual Studio 2022](https://visualstudio.microsoft.com)
* [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)

## 运行环境
- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)

## 使用方式

[* [示例项目](Biwen.Settings/TestWebUI)]

## Easy to Use

### Step 1 

- implements IBiwenSettingsDbContext

```csharp

    public class MyDbContext : DbContext, IBiwenSettingsDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Setting>().HasKey(x => x.SettingName);
            builder.Entity<Setting>().Property(x => x.SettingName).HasMaxLength(500);
            builder.Entity<Setting>().Property(x => x.Order);
        }
    }

```

### step 2

- DI DBContext

  ```csharp
    builder.Services.AddDbContext<MyDbContext>(options =>
    {
        //根据您的情况使用任意EFCore支持的数据库
        //当前使用内存数据库作为演示.生产环境务必修改!
        options.UseInMemoryDatabase("BiwenSettings");
    });
  
   ```

### step 3

- AddBiwenSettings & UseBiwenSettings

```csharp

    builder.Services.AddBiwenSettings(typeof(MyDbContext), options =>
    {
        //自定义布局
        options.Layout = "~/Views/Shared/_Layout.cshtml";
        options.Title = "Biwen.Settings";
        //路由地址 ,http://..../system/settings
        options.Route = "system/settings";
        //授权规则
        options.Valider = (context) =>
        {
            return true;
        };
    });

   //...............

   app.UseBiwenSettings();
```

### Enjoy!

```csharp
    //模拟的配置项,注意描述信息,以及默认值.初始化将以默认值为准
    [Description("微信配置")]
    public class WeChatSetting : SettingBase
    {
        [Description("AppId")]
        public string AppId { get; set; } = "wx1234567890";

        [Description("AppSecret")]
        public string AppSecret { get; set; } = "1234567890";

        [Description("Token")]
        public string Token { get; set; } = "1234567890";

        [Description("EncodingAESKey")]
        public string EncodingAESKey { get; set; } = "1234567890";

        public override int Order => 999;
    }

    //anywhere you can inject
    //View:
    //@inject Settings.WeChatSetting WeChatSetting;

    //Service:
    //public class MyClass
    //{
    //    private readonly WeChatSetting _weChatSetting;
    //    public MyClass(WeChatSetting weChatSetting)
    //    {
    //        _weChatSetting = weChatSetting;
    //    }
    //}

```

## License 
- MIT


## 项目地址

- [GitHub][(https://github.com/vipwan)](https://github.com/vipwan/Biwen.Settings)
