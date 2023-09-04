using Microsoft.EntityFrameworkCore;

namespace Biwen.Settings.SettingManagers.EFCore
{
    public interface IBiwenSettingsDbContext
    {
        /// <summary>
        /// DbSet
        /// </summary>
        DbSet<Setting> Settings { get; set; }
    }
}