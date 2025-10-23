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

    private readonly Stopwatch _session = new();

    public ObservableCollection<Toy> Toys { get; } = new();

    double _catX;
    public double CatX { get => _catX; set { if (_catX != value) { _catX = value; OnPropertyChanged(nameof(CatX)); } } }

    double _catY;
    public double CatY { get => _catY; set { if (_catY != value) { _catY = value; OnPropertyChanged(nameof(CatY)); } } }

    int _score;
    public int Score { get => _score; set { if (_score != value) { _score = value; OnPropertyChanged(nameof(Score)); } } }

    int _toysCaught;
    public int ToysCaught { get => _toysCaught; set { if (_toysCaught != value) { _toysCaught = value; OnPropertyChanged(nameof(ToysCaught)); } } }

    bool _isOutside;
    public bool IsOutside
    {
        get => _isOutside;
        set { if (_isOutside != value) { _isOutside = value; OnPropertyChanged(nameof(IsOutside)); } }
    }

    double _roomX, _roomY;
    double _outX, _outY;

    public ICommand TapMoveCommand { get; }
    public ICommand DropToyCommand { get; }
    public ICommand ToggleLocationCommand { get; }

    public GameViewModel(IDatabaseService db, IAudioService audio, ISettingsService settings)
    {
        _db = db; _audio = audio; _settings = settings;
        _audio.IsEnabled = _settings.SoundEnabled;

        TapMoveCommand = new Command<Point>(async p => await MoveCatAsync(p));
        DropToyCommand = new Command<Toy>(async toy => await CatchToyAsync(toy));
        ToggleLocationCommand = new Command(async () => await ToggleLocationAsync());
    }

    public async Task InitAsync()
    {
        await _db.InitAsync();
        var toys = await _db.GetToysAsync();
        Toys.Clear();
        foreach (var t in toys) Toys.Add(t);

        IsOutside = false;
        _roomX = CatX; _roomY = CatY;
        _audio.StopLoop();
        _session.Restart();
    }

    public async Task SaveStatsAsync()
    {
        _session.Stop();
        _audio.StopLoop();

        var stat = await _db.GetStatAsync();
        stat.Score += Score;
        stat.ToysCaught += ToysCaught;
        stat.TotalPlaySeconds += (long)_session.Elapsed.TotalSeconds;
        stat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveStatAsync(stat);
    }

    async Task MoveCatAsync(Point p)
    {
        await _audio.PlayAsync("meow.mp3");
    }

    async Task CatchToyAsync(Toy toy)
    {
        ToysCaught++;
        Score += toy.Fun;
        await _audio.PlayAsync("purr.mp3");
    }

    async Task ToggleLocationAsync()
    {
        if (IsOutside) { _outX = CatX; _outY = CatY; }
        else { _roomX = CatX; _roomY = CatY; }

        IsOutside = !IsOutside;

        var targetX = IsOutside ? (_outX != 0 ? _outX : CatX) : (_roomX != 0 ? _roomX : CatX);
        var targetY = IsOutside ? (_outY != 0 ? _outY : CatY) : (_roomY != 0 ? _roomY : CatY);

        CatX = targetX; OnPropertyChanged(nameof(CatX));
        CatY = targetY; OnPropertyChanged(nameof(CatY));

        if (IsOutside) await _audio.PlayLoopAsync("birdsongs.mp3", 0.35);
        else _audio.StopLoop();
    }
}
