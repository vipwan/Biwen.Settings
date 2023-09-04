using Biwen.Settings.Domains;
using Biwen.Settings.SettingManagers.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Biwen.Settings.TestWebUI.Data
{
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
        }


    }
}