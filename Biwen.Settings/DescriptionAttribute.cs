namespace Biwen.Settings
{

    /// <summary>
    /// 描述信息 用于Setting,和Setting的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DescriptionAttribute : Attribute
    {
        public string? Description { get; set; }
        public DescriptionAttribute(string? description)
        {
            Description = description;
        }
    }
}
