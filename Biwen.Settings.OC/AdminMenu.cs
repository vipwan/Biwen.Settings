using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.Settings.OC
{
    public class AdminMenu : INavigationProvider
    {

        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
            .Add(S["Configuration"], design => design
                    .Add(S["BiwenSettings"], S["BiwenSettings"].PrefixPosition(), admin => admin
                    .AddClass("biwensettings").Id("biwensettings")
                        .Url("Admin/Biwen.Settings.OC/Home/Setting")
                        //.Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "biwensettings" })
                        .Permission(Permissions.ManageSettings)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;

        }
    }
}