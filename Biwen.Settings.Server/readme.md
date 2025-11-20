```csharp

//修改Program.cs 中的数据库选型
builder.Services.AddDbContext<MyDbContext>(options =>
{
    //options.UseInMemoryDatabase("BiwenSettings");
    options.UseSqlite("Data Source=BiwenSettings.db");
});

```


```bash

# 直接运行 
dotnet run

```