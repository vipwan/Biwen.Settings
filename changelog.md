## ChangeLog

### 1.3.1
    - 提供JsonFile的ISettingManager支持.

### 1.3.0
    - 提供INotify<T> 订阅配置变更

### 1.2.2
	- 提供ISettingManager 自定义持久层

### 1.2.1
	- 提供CachingProvider自定义功能,系统自带NullCacheProvider,MemoryCacheProvider

### 1.2.0
	- 修复了在某些情况下，重命名SettingName后，无法获取到正确的Setting的问题
	- 修改Domain仓储,使用SettingType替代SettingName作为主键

### 1.1.2
- 列表页面的分页功能
- 页面显示参数Options

### 1.1.0
- 添加自动验证功能