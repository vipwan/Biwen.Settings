using Biwen.Settings.Domains;
using Biwen.Settings.SettingStores.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Biwen.Settings.Server.Data
{
    public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options), IBiwenSettingsDbContext
    {
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }


    }
}