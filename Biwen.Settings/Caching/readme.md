#### 使用方式

```csharp

        //支持缓存提供者,默认不使用缓存
        options.UseCacheOfNull();
        //您也可以使用Biwen.Settings提供内存缓存:MemoryCacheProvider
        //options.UseCacheOfMemory();
        //使用自定义缓存提供者
        //options.UseCache<T>();

```