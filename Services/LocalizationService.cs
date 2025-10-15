using System.Globalization;
using CatWorld.Resources.Localization;

namespace CatWorld.Services;

public interface ILocalizationService
{
    CultureInfo CurrentCulture { get; }
    void SetCulture(string cultureName); // "en", "et"
    event EventHandler? LanguageChanged;
}

public class LocalizationService : ILocalizationService
{
    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;
    public event EventHandler? LanguageChanged;

    public void SetCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        Preferences.Set("lang", cultureName);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
