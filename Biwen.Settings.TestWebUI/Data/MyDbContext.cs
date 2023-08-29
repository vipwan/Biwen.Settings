using Biwen.Settings.Domains;
using Biwen.Settings.EntityFramework;
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


            builder.Entity<Setting>().HasKey(x => x.SettingName);
            builder.Entity<Setting>().Property(x => x.SettingName).HasMaxLength(500);
            builder.Entity<Setting>().Property(x => x.Order);
        }


    }
}