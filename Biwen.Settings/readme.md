# Biwen.Settings

![image](https://github.com/vipwan/Biwen.Settings/assets/13956765/b438393b-f5d9-4d78-b2aa-c20851fb9880)

## 开源动机
- 由于当前有好几个小项目,且每个小项目都有比较多的配置项,所以把这个模块抽离出来打包到Nuget共用.
- 每个小项目以及模块可以在任意想要动态配置的地方定义Setting, 系统会直接注入到自己的业务代码中且存储到持久层 所见即所得.我感觉有部分用户有这样的需求.


## NuGet 包

- dotnet add package Biwen.Settings --version 1.0.2

## 开发环境

* Windows 10
* [Rider 2022](https://www.jetbrains.com/rider) / [Visual Studio 2022](https://visualstudio.microsoft.com) / [Visual Studio Code](https://code.visualstudio.com)
* [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)
  

## 运行环境
- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)

## 使用方式

[* [示例项目](https://github.com/vipwan/Biwen.Settings/tree/master/Biwen.Settings.TestWebUI)]

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
            //可以根据自己的情况约束存储列
            //builder.Entity<Setting>().HasKey(x => x.SettingName);
            //builder.Entity<Setting>().Property(x => x.SettingName).HasMaxLength(500);
        }
    }

```
#### 1.1 Migration

- Add a new Entity Framework Core migration to save your changes

```bash
dotnet ef migrations add biwenSettings
dotnet ef database update
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
//ProjectId : 项目标识 用于区分不同的项目,比如:日志系统,文件系统;或者环境,比如:开发环境,测试环境,生产环境
#if DEBUG
    options.ProjectId = $"Biwen.Settings.TestWebUI-{"Development"}";
#endif
#if !DEBUG
    options.ProjectId = $"Biwen.Settings.TestWebUI-{"Production"}";
#endif
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

## 联系我
- QQ:552175420
- Email: vipwan#sina.com

## 项目地址

- [GitHub][(https://github.com/vipwan)](https://github.com/vipwan/Biwen.Settings)
