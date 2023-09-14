using Microsoft.Extensions.Localization;

namespace Biwen.Settings
{


    /// <summary>
    /// IStringLocalizer<T> Or BiwenSettingsLocalizer.T
    /// </summary>
    public class BiwenSettingsLocalizer
    {
        /// <summary>
        /// UI Localization
        /// </summary>
        public IStringLocalizer<UI> UI { get; private set; }

        public BiwenSettingsLocalizer(IStringLocalizer<UI> ui)
        {
            UI = ui;
        }
    }
}