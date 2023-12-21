using OrchardCore.Security.Permissions;

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
                new() {
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