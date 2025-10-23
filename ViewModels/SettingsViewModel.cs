using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly ISettingsService _settings;

    public SettingsViewModel(ISettingsService settings)
    {
        _settings = settings;

        // Подписываемся на изменение настроек
        _settings.SettingsChanged += (_, _) => OnPropertyChanged(null);

        SaveCommand = new Command(Save);
        ResetCommand = new Command(ResetDefaults);
    }

    // ==== Настройки ====

    public bool SoundEnabled
    {
        get => _settings.SoundEnabled;
        set { _settings.SoundEnabled = value; OnPropertyChanged(); }
    }

    public double Volume
    {
        get => _settings.Volume;
        set { _settings.Volume = value; OnPropertyChanged(); }
    }

    public string LanguageCode
    {
        get => _settings.LanguageCode;
        set { _settings.LanguageCode = value; OnPropertyChanged(); }
    }

    public TimeSpan NightStart
    {
        get => _settings.NightStart;
        set { _settings.NightStart = value; OnPropertyChanged(); }
    }

    public TimeSpan NightEnd
    {
        get => _settings.NightEnd;
        set { _settings.NightEnd = value; OnPropertyChanged(); }
    }

    // ==== Команды ====
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }

    void Save()
    {
        _settings.Flush();
        _settings.RaiseSettingsChanged();
    }

    void ResetDefaults()
    {
        _settings.SoundEnabled = true;
        _settings.Volume = 0.8;
        _settings.LanguageCode = "en";
        _settings.NightStart = new TimeSpan(21, 0, 0);
        _settings.NightEnd = new TimeSpan(8, 0, 0);

        _settings.Flush();
        _settings.RaiseSettingsChanged();

        OnPropertyChanged(null);
    }
}
