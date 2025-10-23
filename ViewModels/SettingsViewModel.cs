using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly ISettingsService _settings;
    private readonly ILocalizationService _loc;

    public SettingsViewModel(ISettingsService settings, ILocalizationService loc)
    {
        _settings = settings;
        _loc = loc;

        // загрузить сохранённые значения
        SelectedLanguage = _settings.LanguageCode;          // "et"/"en"/"ru"
        NightStart = _settings.NightStart;                  // TimeSpan
        NightEnd = _settings.NightEnd;

        SetLanguageCommand = new Command<string>(code => { SelectedLanguage = code; ApplyLanguage(); SaveLanguage(); });
        SaveNightCommand = new Command(SaveNightSchedule);
    }

    // ===== Язык =====
    string _selectedLanguage = "en";
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set { if (_selectedLanguage != value) { _selectedLanguage = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentLanguageDisplay)); } }
    }

    public string CurrentLanguageDisplay => SelectedLanguage switch
    {
        "et" => "Eesti keel",
        "ru" => "Русский",
        _ => "English",
    };

    public ICommand SetLanguageCommand { get; }

    void ApplyLanguage()
    {
        // применяем сразу
        _loc.SetCulture(SelectedLanguage);
    }

    void SaveLanguage()
    {
        _settings.LanguageCode = SelectedLanguage;
        _settings.Flush();
    }

    // ===== Ночной режим (расписание) =====
    TimeSpan _nightStart;
    public TimeSpan NightStart
    {
        get => _nightStart;
        set { if (_nightStart != value) { _nightStart = value; OnPropertyChanged(); } }
    }

    TimeSpan _nightEnd;
    public TimeSpan NightEnd
    {
        get => _nightEnd;
        set { if (_nightEnd != value) { _nightEnd = value; OnPropertyChanged(); } }
    }

    string _saveStatus = "";
    public string SaveStatus { get => _saveStatus; set { _saveStatus = value; OnPropertyChanged(); } }

    public ICommand SaveNightCommand { get; }

    void SaveNightSchedule()
    {
        _settings.NightStart = NightStart;
        _settings.NightEnd = NightEnd;
        _settings.Flush();
        SaveStatus = $"Ночной режим: {NightStart:hh\\:mm}-{NightEnd:hh\\:mm} ✓";

        // пинганём слушателей (например, GameViewModel), что расписание изменилось
        _settings.RaiseSettingsChanged();
    }
}
