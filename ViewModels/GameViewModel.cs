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

    // Положение кота сразу после выхода на улицу (TranslationX / TranslationY)
    public double OutsideStandX { get; set; } = 0;      // по центру
    public double OutsideStandY { get; set; } = 250;    // ниже центра (подбери под свой фон/спрайт)

    // Старт мини-игры на улице: X и фиксированная «дорожка» по Y
    public double MiniStartX { get; set; } = 0;       // по центру
    public double MiniTrackY { get; set; } = 300;     // высота дорожки (TranslationY)

    // -------- Позиция кота --------
    double _catX;
    public double CatX { get => _catX; set { if (_catX != value) { _catX = value; OnPropertyChanged(nameof(CatX)); } } }

    double _catY;
    public double CatY { get => _catY; set { if (_catY != value) { _catY = value; OnPropertyChanged(nameof(CatY)); } } }

    int _score;
    public int Score { get => _score; set { if (_score != value) { _score = value; OnPropertyChanged(nameof(Score)); } } }

    int _toysCaught;
    public int ToysCaught { get => _toysCaught; set { if (_toysCaught != value) { _toysCaught = value; OnPropertyChanged(nameof(ToysCaught)); } } }

    // -------- Локация: Комната / Улица --------
    bool _isOutside;
    public bool IsOutside
    {
        get => _isOutside;
        set { if (_isOutside != value) { _isOutside = value; OnPropertyChanged(nameof(IsOutside)); } }
    }

    // Последняя позиция кота в каждой локации (если нужна)
    double _roomX, _roomY;
    double _outX, _outY;

    // -------- Мини-игра (на улице) --------
    bool _isMiniGame;
    public bool IsMiniGame
    {
        get => _isMiniGame;
        set { if (_isMiniGame != value) { _isMiniGame = value; OnPropertyChanged(nameof(IsMiniGame)); } }
    }

    int _miniScore;
    public int MiniScore { get => _miniScore; set { if (_miniScore != value) { _miniScore = value; OnPropertyChanged(nameof(MiniScore)); } } }

    int _miniLives = 3;
    public int MiniLives { get => _miniLives; set { if (_miniLives != value) { _miniLives = value; OnPropertyChanged(nameof(MiniLives)); } } }

    // -------- Команды --------
    public ICommand TapMoveCommand { get; }
    public ICommand DropToyCommand { get; }
    public ICommand ToggleLocationCommand { get; }
    public ICommand ToggleMiniGameCommand { get; }

    public GameViewModel(IDatabaseService db, IAudioService audio, ISettingsService settings)
    {
        _db = db; _audio = audio; _settings = settings;
        _audio.IsEnabled = _settings.SoundEnabled;

        TapMoveCommand = new Command<Point>(async p => await MoveCatAsync(p));
        DropToyCommand = new Command<Toy>(async toy => await CatchToyAsync(toy));
        ToggleLocationCommand = new Command(async () => await ToggleLocationAsync());

        // запуск / стоп мини-игры (только на улице)
        ToggleMiniGameCommand = new Command(() =>
        {
            if (!IsOutside) return;        // играть можно только на улице
            IsMiniGame = !IsMiniGame;
            if (IsMiniGame) { MiniScore = 0; MiniLives = 3; }
        });
    }

    public async Task InitAsync()
    {
        await _db.InitAsync();
        var toys = await _db.GetToysAsync();
        Toys.Clear();
        foreach (var t in toys) Toys.Add(t);

        // стартуем с комнаты
        IsOutside = false;
        _roomX = CatX; _roomY = CatY;

        // на всякий случай выключим любые фоновые звуки
        _audio.StopLoop();

        _session.Restart();
    }

    public async Task SaveStatsAsync()
    {
        _session.Stop();
        _audio.StopLoop(); // глушим фон при уходе

        var stat = await _db.GetStatAsync();
        stat.Score += Score;
        stat.ToysCaught += ToysCaught;
        stat.TotalPlaySeconds += (long)_session.Elapsed.TotalSeconds;
        stat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveStatAsync(stat);
    }

    async Task MoveCatAsync(Point p)
    {
        // короткий эффект (по желанию)
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
        // запомним позицию текущей локации
        if (IsOutside) { _outX = CatX; _outY = CatY; }
        else { _roomX = CatX; _roomY = CatY; }

        // переключаемся
        IsOutside = !IsOutside;

        if (IsOutside)
        {
            // применяем ТВОИ координаты стояния на улице
            CatX = OutsideStandX;
            CatY = OutsideStandY;

            await _audio.PlayLoopAsync("birdsongs.mp3", 0.35);
        }
        else
        {
            _audio.StopLoop();

            // вернём кота туда, где он был в комнате (или оставим как есть)
            var targetX = _roomX != 0 ? _roomX : CatX;
            var targetY = _roomY != 0 ? _roomY : CatY;
            CatX = targetX; OnPropertyChanged(nameof(CatX));
            CatY = targetY; OnPropertyChanged(nameof(CatY));

            // выключаем мини-игру, если была
            if (IsMiniGame) IsMiniGame = false;
        }
    }
}
