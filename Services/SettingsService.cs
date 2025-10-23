using Microsoft.Maui.Storage;

namespace CatWorld.Services;

public class SettingsService : ISettingsService
{
    // Ключи для Preferences
    const string KeySoundEnabled = "SoundEnabled";
    const string KeyVolume = "Volume";
    const string KeyLang = "LanguageCode";

    const string KeyNightStart = "NightStartMin"; // минуты с 00:00
    const string KeyNightEnd = "NightEndMin";

    const string KeyMiniStartX = "MiniStartX";
    const string KeyMiniTrackY = "MiniTrackY";

    public bool SoundEnabled
    {
        get => Preferences.Get(KeySoundEnabled, true);
        set { Preferences.Set(KeySoundEnabled, value); RaiseSettingsChanged(); }
    }

    public double Volume
    {
        get => Preferences.Get(KeyVolume, 0.7);
        set { Preferences.Set(KeyVolume, Math.Clamp(value, 0, 1)); RaiseSettingsChanged(); }
    }

    public string LanguageCode
    {
        get => Preferences.Get(KeyLang, "en");
        set { Preferences.Set(KeyLang, string.IsNullOrWhiteSpace(value) ? "en" : value); RaiseSettingsChanged(); }
    }

    public TimeSpan NightStart
    {
        get => TimeSpan.FromMinutes(Preferences.Get(KeyNightStart, 21 * 60)); // 21:00
        set { Preferences.Set(KeyNightStart, (int)value.TotalMinutes); RaiseSettingsChanged(); }
    }

    public TimeSpan NightEnd
    {
        get => TimeSpan.FromMinutes(Preferences.Get(KeyNightEnd, 8 * 60)); // 08:00
        set { Preferences.Set(KeyNightEnd, (int)value.TotalMinutes); RaiseSettingsChanged(); }
    }

    public double MiniStartX
    {
        get => Preferences.Get(KeyMiniStartX, 0.0);
        set { Preferences.Set(KeyMiniStartX, value); RaiseSettingsChanged(); }
    }

    public double MiniTrackY
    {
        get => Preferences.Get(KeyMiniTrackY, 120.0);
        set { Preferences.Set(KeyMiniTrackY, value); RaiseSettingsChanged(); }
    }

    public void Flush() { /* Preferences пишут сразу; ничего делать не нужно */ }

    public event EventHandler? SettingsChanged;
    public void RaiseSettingsChanged() => SettingsChanged?.Invoke(this, EventArgs.Empty);
}
