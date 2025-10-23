using Microsoft.Maui.Storage;

namespace CatWorld.Services;

public class SettingsService : ISettingsService
{
    public bool SoundEnabled
    {
        get => Preferences.Get(nameof(SoundEnabled), true);
        set => Preferences.Set(nameof(SoundEnabled), value);
    }

    public double Volume
    {
        get => Preferences.Get(nameof(Volume), 0.8);
        set => Preferences.Set(nameof(Volume), value);
    }

    public string LanguageCode
    {
        get => Preferences.Get(nameof(LanguageCode), "en");
        set => Preferences.Set(nameof(LanguageCode), value ?? "en");
    }

    // храним в минутах от полуночи
    public TimeSpan NightStart
    {
        get => TimeSpan.FromMinutes(Preferences.Get(nameof(NightStart) + "_min", 21 * 60));
        set => Preferences.Set(nameof(NightStart) + "_min", (int)value.TotalMinutes);
    }

    public TimeSpan NightEnd
    {
        get => TimeSpan.FromMinutes(Preferences.Get(nameof(NightEnd) + "_min", 8 * 60));
        set => Preferences.Set(nameof(NightEnd) + "_min", (int)value.TotalMinutes);
    }

    public double MiniStartX
    {
        get => Preferences.Get(nameof(MiniStartX), 0.0);
        set => Preferences.Set(nameof(MiniStartX), value);
    }

    public double MiniTrackY
    {
        get => Preferences.Get(nameof(MiniTrackY), 0.0);
        set => Preferences.Set(nameof(MiniTrackY), value);
    }

    public void Flush() { /* Preferences уже сохраняет */ }

    public event EventHandler? SettingsChanged;
    public void RaiseSettingsChanged() => SettingsChanged?.Invoke(this, EventArgs.Empty);
}
