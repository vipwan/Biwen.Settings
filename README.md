# Biwen.Settings
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
        options.UseInMemoryDatabase("BiwenSettings");
    });
  
   ```

### step 3

- AddBiwenSettings & UseBiwenSettings

  ```csharp

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

   //...............

   app.UseBiwenSettings();

```

## 项目地址

- [GitHub][(https://github.com/vipwan/Biwen)](https://github.com/vipwan/Biwen.Settings)
