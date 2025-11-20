namespace Biwen.Settings.Server.Services;

/// <summary>
/// 测试服务,返回站点名称
/// </summary>
/// <param name="setting"></param>
public class TestService(SiteSetting setting)
{
    public string? GetSiteName()
    {
        return setting.SiteName;
    }
}
