namespace CatWorld.Services;

public interface ISettingsService
{
    // Уже существующие/нужные настройки
    bool SoundEnabled { get; set; }
    double Volume { get; set; }
    string LanguageCode { get; set; }   // "en" | "et" | "ru"

    // Ночной режим по расписанию
    TimeSpan NightStart { get; set; }   // по умолчанию 21:00
    TimeSpan NightEnd { get; set; }   // по умолчанию 08:00

    // Мини-игра (улица)
    double MiniStartX { get; set; }
    double MiniTrackY { get; set; }

    // На Preferences можно не писать явно, но оставим для совместимости
    void Flush();

    // Сигнал «настройки изменились» (чтобы VM могли пересчитать состояние)
    event EventHandler? SettingsChanged;
    void RaiseSettingsChanged();
}
