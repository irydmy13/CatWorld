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
        // ��� ����:
        bool SoundEnabled { get; set; }
        double Volume { get; set; }
        string LanguageCode { get; set; }

        // ���������� ����:
        TimeSpan NightStart { get; set; }   // �� ���������: 21:00
        TimeSpan NightEnd { get; set; }   // �� ���������: 08:00

        // ����-����
        double MiniStartX { get; set; }
        double MiniTrackY { get; set; }

        void Flush();

        // �������, ����� VM ����� ������������� (����������� ����)
        event EventHandler? SettingsChanged;
        void RaiseSettingsChanged();
    }
}