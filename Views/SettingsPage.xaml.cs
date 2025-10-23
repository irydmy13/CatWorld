using CatWorld.ViewModels;

namespace CatWorld.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    public interface ISettingsService
    {
        // уже были:
        bool SoundEnabled { get; set; }
        double Volume { get; set; }
        string LanguageCode { get; set; }

        // расписание ночи:
        TimeSpan NightStart { get; set; }   // по умолчанию: 21:00
        TimeSpan NightEnd { get; set; }   // по умолчанию: 08:00

        // мини-игра
        double MiniStartX { get; set; }
        double MiniTrackY { get; set; }

        void Flush();

        // событие, чтобы VM могли отреагировать (пересчитать ночь)
        event EventHandler? SettingsChanged;
        void RaiseSettingsChanged();
    }
}