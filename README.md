# Biwen.Settings

![Nuget](https://img.shields.io/nuget/v/Biwen.Settings)
![Nuget](https://img.shields.io/nuget/dt/Biwen.Settings)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/vipwan/Biwen.Settings/blob/master/LICENSE.txt) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vipwan/Biwen.Settings/pulls) 

## Biwen.Settings介绍

解决程序配置项重复引用的问题,比如我们有一个GitHubSetting配置项,
我们可以在任意地方注入GitHubSetting,系统会自动将配置项注入到我们的业务代码中,并且可以在系统中动态修改配置项,
修改后会自动持久化到存储中,并通知集群的订阅子节点Setting配置项变更事件,清空缓存并重新加载持久层的最新项 [案例项目:BiwenSettingsMutiNodeTest](https://github.com/vipwan/BiwenSettingsMutiNodeTest)

当前仓储可以是一个抽象的概念,可以任意横向实现.比如使用多个数据库负载,或者多个Redis等.只要能保证数据一致性即可

![image](https://github.com/vipwan/Biwen.Settings/assets/13956765/e27cbca0-9c3d-4851-8aa1-37d2ce1ac97d)

## 基础案例

- 一个简单例子 比如我有一个Web站点,这个Web站点有很多的负载,以及一一些小程序后台服务 , 
- 这些独立的程序(或者裂变或者使用代理转发的微服务), 都使用同一个 WX账号收账,那么这些站点或者后台服务都可以是这个WX账号配置的消费节点,
- 这些节点可以配置一个或者多个作为主节点,当任意一个主节点修改配置项时,主动通知其他子节点更新配置,并且清空配置的缓存服务并重新加载Store!

## NuGet 包

- dotnet add package Biwen.Settings


## 开发环境

* Windows 10
* [Rider 2022](https://www.jetbrains.com/rider) / [Visual Studio 2022](https://visualstudio.microsoft.com) / [Visual Studio Code](https://code.visualstudio.com)
* [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/7.0)
  
## 运行环境
- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
## 使用方式

[* [示例项目](https://github.com/vipwan/Biwen.Settings/tree/master/Biwen.Settings.TestWebUI)]

## Easy to Use

### Step 1 

### 1.1 使用EntityFrameworkCore的方式

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
            //根据情况自定义表名
            builder.Entity<Setting>(b =>
            {
                b.ToTable("Settings");
            });
            //可以根据自己的情况约束存储列
            //builder.Entity<Setting>().HasKey(x => x.SettingName);
            //builder.Entity<Setting>().Property(x => x.SettingName).HasMaxLength(500);
        }
    }

```
#### 1.1.1 Migration

- Add a new Entity Framework Core migration to save your changes

```bash
dotnet ef migrations add biwenSettings
dotnet ef database update
```

### 1.2 使用自定义SettingManager

- 直接跳入step 2.2

### step 2


- 2.1 如果使用Biwen.Settings提供的EF仓储,必须注入DBContext

  ```csharp
    builder.Services.AddDbContext<MyDbContext>(options =>
    {
        //根据您的情况使用任意EFCore支持的数据库
        //当前使用内存数据库作为演示.生产环境务必修改!
        options.UseInMemoryDatabase("BiwenSettings");
    });
  
   ```

- 2.2 如果使用自定义仓储,请实现ISettingManager并修改AddBiwenSettings()
- 
  ```csharp

     options.UseSettingManager<T,V>()
  
   ```

### step 3

- AddBiwenSettings & UseBiwenSettings

```csharp

    builder.Services.AddBiwenSettings(options =>
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
        options.HasPermission = (ctx) => true;
        options.EditorOption.EditorOnclick = "return confirm('Are You Sure!?');";
        options.EditorOption.EdtiorConfirmButtonText = "Submit";
        options.EditorOption.EditorEditButtonText = "Edit";
        options.EditorOption.ShouldPagenation = true;

        //开启AutoFluentValidation
        options.AutoFluentValidationOption.Enable = true;

        //支持缓存提供者,默认不使用缓存
        options.UseCacheOfNull();
        //您也可以使用Biwen.Settings提供内存缓存:MemoryCacheProvider
        //options.UseCacheOfMemory();
        //使用自定义缓存提供者
        //options.UseCache<T>();
        //配置当前服务为主节点
        options.NotifyOption.IsNotifyEnable = true;
        options.NotifyOption.Secret = "Biwen.Settings.Notify";
        //子节点配置
        options.NotifyOption.EndpointHosts = new[]
        {
            "http://localhost:5150"
        };
        //默认提供EntityFrameworkCore持久化配置项 dbContextType必须配置
        //options.UseStoreOfEFCore(options =>
        //{
        //    options.DbContextType = typeof(MyDbContext);
        //});
        //使用JsonStore持久化配置项
        options.UserStoreOfJsonFile(options =>
        {
            options.FormatJson = true;
            options.JsonPath = "systemsetting.json";
        });
        
        //自行实现的ISettingManager注册
        //options.UseSettingManager<T,V>()
    });

   //...............
   app.UseBiwenSettings(mapNotifyEndpoint: true, builder: builder =>
    {
        builder.WithTags("BiwenSettingApi").WithOpenApi();
    });

```

### Enjoy!

#### 支持提交更新时验证器自动验证
![image](https://github.com/vipwan/Biwen.Settings/assets/13956765/e2663e78-61da-43f1-990d-aa970736f023)
![image](https://github.com/vipwan/Biwen.Settings/assets/13956765/40399554-90be-4927-8b03-a516614d4bd6)

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
        //排序
        public override int Order => 500;
        //定义验证器
        public class WeChatSettingValidtor : AbstractValidator<WeChatSetting>
        {
            public WeChatSettingValidtor()
            {
                //验证规则
                RuleFor(x => x.AppId).NotEmpty().Length(12, 32);
                RuleFor(x => x.AppSecret).NotNull().NotEmpty().Length(12, 128);
            }
        }
    }

    /// <summary>
    /// 内置验证器的配置项,推荐使用这种方式 ,同时支持`FluentValidation`和`DataAnnotations`
    /// </summary>
    [Description("内置验证器的配置项测试")]
    public class TestAutoValidSetting : ValidationSettingBase<TestAutoValidSetting>
    {
        [StringLength(50, MinimumLength = 3)] // DataAnnotations支持
        public string Name { get; set; } = "Hello"!;

        public TestAutoValidSetting()
        {
            //构造函数中添加验证规则
            RuleFor(x => x.Name).NotEmpty().Length(8, 32);
        }
        override public int Order => 600;
    }

    //anywhere you can inject
    //View:
    //@inject WeChatSetting WeChatSetting;
    //@inject TestAutoValidSetting TestAutoValidSetting;

    //Service:
    //public class MyClass
    //{
    //    private readonly WeChatSetting _weChatSetting;
    //    private readonly TestAutoValidSetting _testAutoValidSetting;
    //    public MyClass(WeChatSetting weChatSetting,TestAutoValidSetting testAutoValidSetting)
    //    {
    //        _weChatSetting = weChatSetting;
    //        _testAutoValidSetting = testAutoValidSetting;
    //    }
    //}

```

#### More

- INotify订阅配置变更

```csharp

        public class WeChatSettingNotify : INotify<WeChatSetting>
        {
            private readonly ILogger<WeChatSettingNotify> _logger;

            public WeChatSettingNotify(ILogger<WeChatSettingNotify> logger)
            {
                _logger = logger;
            }

            public async Task Notify(WeChatSetting setting)
            {
                _logger.LogInformation("微信配置发生变更!");
                await Task.CompletedTask;
            }
        }
```

- Minimal API

```csharp

//all settings
//GET ~/{RoutePrefix}/all

//get setting by full type name
//GET ~/{RoutePrefix}/get/{id}

//update setting by full type name
//POST ~/{RoutePrefix}/set/{id}
//body:
//{
//    "PropertyName1": "...",
//    "PropertyName2": "...",
//    "PropertyName3": "..."
//}

```


## License 
- MIT

## 联系我
- QQ:552175420
- Email: vipwan#sina.com

## 项目地址

- [GitHub][(https://github.com/vipwan)](https://github.com/vipwan/Biwen.Settings)
