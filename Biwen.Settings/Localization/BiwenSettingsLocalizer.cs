namespace Biwen.Settings
{
    /// <summary>
    /// IStringLocalizer<T> Or BiwenSettingsLocalizer.T
    /// </summary>
    public class BiwenSettingsLocalizer(IStringLocalizer<UI> ui)
    {
        /// <summary>
        /// UI Localization
        /// </summary>
        public IStringLocalizer<UI> UI { get; private set; } = ui;
    }
}