using OrchardCore.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.Settings.OC
{
    public class Permissions : IPermissionProvider
    {
        /// <summary>
        /// 管理Settings的权限
        /// </summary>
        public static readonly Permission ManageSettings = new("ManageSettings", "Manage Settings", true);

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new List<PermissionStereotype>
           {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageSettings }
                }
            };
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageSettings }.AsEnumerable());
        }
    }
}