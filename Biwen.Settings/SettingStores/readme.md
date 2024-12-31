#### 使用方式

```csharp

//默认提供EntityFrameworkCore持久化配置项
//options.UseStoreOfEFCore<MyDbContext>();

//使用JsonStore持久化配置项
options.UserSettingStoreOfJsonStore(options =>
{
    options.FormatJson = true;
    options.JsonPath = "systemsetting.json";
});
        
//自行实现的ISettingStore注册
//options.UseSettingStore<T,V>()

```