// Licensed to the Biwen.Settings.OC under one or more agreements.
// The Biwen.Settings.OC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Navigation;

namespace Biwen.Settings.OC
{
    public class AdminMenu(IStringLocalizer<AdminMenu> localizer, IOptions<AdminOptions> options) : INavigationProvider
    {

        private readonly IStringLocalizer S = localizer;
        private readonly IOptions<AdminOptions> _options = options;

        public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return ValueTask.CompletedTask;
            }

            builder
            .Add(S["Configuration"], design => design
                    .Add(S["BiwenSettings"], S["BiwenSettings"].PrefixPosition(), admin => admin
                    .AddClass("biwensettings").Id("biwensettings")
                        .Url($"{_options.Value.AdminUrlPrefix}/Biwen.Settings.OC/Home/Setting")
                        //.Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "biwensettings" })
                        .Permission(Permissions.ManageSettings)
                        .LocalNav()
                    )
                );

            return ValueTask.CompletedTask;

        }
    }
}