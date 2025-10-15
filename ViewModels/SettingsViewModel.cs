using System.ComponentModel;
using System.Windows.Input;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly IThemeService _theme;
    private readonly ILocalizationService _loc;
    private readonly ISettingsService _settings;
    private readonly IAudioService _audio;

    public string SelectedLanguage
    {
        get => _settings.Language;
        set { if (_settings.Language != value) { _settings.Language = value; _loc.SetCulture(value); OnPropertyChanged(nameof(SelectedLanguage)); } }
    }

    public bool SoundEnabled
    {
        get => _settings.SoundEnabled;
        set { if (_settings.SoundEnabled != value) { _settings.SoundEnabled = value; _audio.IsEnabled = value; OnPropertyChanged(nameof(SoundEnabled)); } }
    }

    public IEnumerable<string> Languages => new[] { "en", "et" };

    public ICommand SetLightCommand { get; }
    public ICommand SetDarkCommand { get; }

    public SettingsViewModel(IThemeService theme, ILocalizationService loc, ISettingsService settings, IAudioService audio)
    {
        _theme = theme; _loc = loc; _settings = settings; _audio = audio;
        _audio.IsEnabled = settings.SoundEnabled;

        SetLightCommand = new Command(() => _theme.Apply(AppTheme.Light));
        SetDarkCommand = new Command(() => _theme.Apply(AppTheme.Dark));
    }
}
