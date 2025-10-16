using CatWorld.Services;
using CatWorld.Views;
using Microsoft.Maui.Controls;
namespace CatWorld;

public partial class App : Application
{
    public App(IThemeService themeService)
    {
        InitializeComponent();
        themeService.Apply(ThemeService.LoadSavedOrDefault());
        MainPage = new AppShell();
    }
}
