using Biwen.Settings.Domains;
using Microsoft.EntityFrameworkCore;

namespace Biwen.Settings.EntityFramework
{
    public interface IBiwenSettingsDbContext
    {
        /// <summary>
        /// DbSet
        /// </summary>
        DbSet<Setting> Settings { get; set; }
    }
}