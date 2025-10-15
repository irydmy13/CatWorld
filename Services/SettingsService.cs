namespace CatWorld.Services;

public interface ISettingsService
{
    string Language { get; set; }   // "en" | "et"
    bool SoundEnabled { get; set; }
}

public class SettingsService : ISettingsService
{
    public string Language
    {
        get => Preferences.Get("lang", "en");
        set => Preferences.Set("lang", value);
    }
    public bool SoundEnabled
    {
        get => Preferences.Get("sound", true);
        set => Preferences.Set("sound", value);
    }
}
