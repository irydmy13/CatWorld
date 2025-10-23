using CatWorld.Services;

namespace CatWorld;

public partial class App : Application
{
    private readonly INightModeService _nightMode;

    public App(INightModeService nightMode)
    {
        InitializeComponent();
        _nightMode = nightMode;

        MainPage = new AppShell();
        ApplyNightThemeIfNeeded();
    }

    private void ApplyNightThemeIfNeeded()
    {
        UserAppTheme = _nightMode.IsNightNow() ? AppTheme.Dark : AppTheme.Light;
    }

    protected override void OnStart()
    {
        base.OnStart();
        ApplyNightThemeIfNeeded();
    }

    protected override void OnResume()
    {
        base.OnResume();
        ApplyNightThemeIfNeeded();
    }
}
