using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CatWorld.Models;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly IDatabaseService _db;
    private readonly IAudioService _audio;
    private readonly ISettingsService _settings;

    private Stopwatch _session = new();

    public ObservableCollection<Toy> Toys { get; } = new();

    // Позиция кота (привязана к XAML)
    double _catX; public double CatX { get => _catX; set { _catX = value; OnPropertyChanged(nameof(CatX)); } }
    double _catY; public double CatY { get => _catY; set { _catY = value; OnPropertyChanged(nameof(CatY)); } }

    int _score; public int Score { get => _score; set { _score = value; OnPropertyChanged(nameof(Score)); } }
    int _toysCaught; public int ToysCaught { get => _toysCaught; set { _toysCaught = value; OnPropertyChanged(nameof(ToysCaught)); } }

    public ICommand TapMoveCommand { get; }
    public ICommand DropToyCommand { get; } // parameter: Toy

    public GameViewModel(IDatabaseService db, IAudioService audio, ISettingsService settings)
    {
        _db = db; _audio = audio; _settings = settings;
        _audio.IsEnabled = _settings.SoundEnabled;

        TapMoveCommand = new Command<Point>(async p => await MoveCatAsync(p));
        DropToyCommand = new Command<Toy>(async toy => await CatchToyAsync(toy));
    }

    public async Task InitAsync()
    {
        await _db.InitAsync();
        var toys = await _db.GetToysAsync();
        Toys.Clear();
        foreach (var t in toys) Toys.Add(t);
        _session.Restart();
    }

    async Task MoveCatAsync(Point p)
    {
        // позиция обновляется анимацией в code-behind; тут можно звук «мяу»
        await _audio.PlayAsync("meow.mp3");
    }

    async Task CatchToyAsync(Toy toy)
    {
        // повысим счёт и радость
        ToysCaught++;
        Score += toy.Fun;
        await _audio.PlayAsync("purr.mp3");
        OnPropertyChanged(nameof(Score));
        OnPropertyChanged(nameof(ToysCaught));
    }

    public async Task SaveStatsAsync()
    {
        _session.Stop();
        var stat = await _db.GetStatAsync();
        stat.Score += Score;
        stat.ToysCaught += ToysCaught;
        stat.TotalPlaySeconds += (long)_session.Elapsed.TotalSeconds;
        stat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveStatAsync(stat);
    }
}
