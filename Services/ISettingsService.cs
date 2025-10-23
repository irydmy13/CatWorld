namespace CatWorld.Services;

public interface ISettingsService
{
    // существующие у тебя поля
    bool SoundEnabled { get; set; }
    double Volume { get; set; }
    string LanguageCode { get; set; }

    // расписание ночи
    TimeSpan NightStart { get; set; }   // по умолчанию 21:00
    TimeSpan NightEnd { get; set; }   // по умолчанию 08:00

    // мини-игра
    double MiniStartX { get; set; }
    double MiniTrackY { get; set; }

    void Flush();

    // нотификация об изменении настроек
    event EventHandler? SettingsChanged;
    void RaiseSettingsChanged();
}
