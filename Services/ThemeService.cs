namespace CatWorld.Services;

public interface IThemeService
{
    void Apply(AppTheme theme);
    AppTheme Current { get; }
}

public class ThemeService : IThemeService
{
    public AppTheme Current { get; private set; } = AppTheme.Light;

    public void Apply(AppTheme theme)
    {
        Application.Current!.UserAppTheme = theme;
        Current = theme;
        Preferences.Set("theme", theme.ToString());
    }

    public static AppTheme LoadSavedOrDefault()
    {
        var saved = Preferences.Get("theme", AppTheme.Light.ToString());
        return Enum.TryParse<AppTheme>(saved, out var t) ? t : AppTheme.Light;
    }
}
