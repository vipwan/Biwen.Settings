#### 使用方式

```csharp

//默认提供EntityFrameworkCore持久化配置项
//options.UseStoreOfEFCore<MyDbContext>();

//使用JsonStore持久化配置项
options.UserSettingManagerOfJsonStore(options =>
{
    options.FormatJson = true;
    options.JsonPath = "systemsetting.json";
});
        
//自行实现的ISettingManager注册
//options.UseSettingManager<T,V>()

```