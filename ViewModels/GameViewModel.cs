using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CatWorld.Models;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly IDatabaseService _db;
    private readonly IAudioService _audio;
    private readonly ISettingsService _settings;

    private readonly Stopwatch _session = new();

    public ObservableCollection<Toy> Toys { get; } = new();

    // ===== Позиция кота (биндится в XAML) =====
    double _catX;
    public double CatX { get => _catX; set { if (_catX != value) { _catX = value; Raise(); } } }

    double _catY;
    public double CatY { get => _catY; set { if (_catY != value) { _catY = value; Raise(); } } }

    int _score;
    public int Score { get => _score; set { if (_score != value) { _score = value; Raise(); } } }

    int _toysCaught;
    public int ToysCaught { get => _toysCaught; set { if (_toysCaught != value) { _toysCaught = value; Raise(); } } }

    // ===== Локация: Комната / Улица =====
    bool _isOutside;
    public bool IsOutside
    {
        get => _isOutside;
        set { if (_isOutside != value) { _isOutside = value; Raise(); } }
    }

    // запоминаем последнюю позицию кота для каждой локации
    double _roomX, _roomY;
    double _outX, _outY;

    // ===== Ночной режим (расчёт из настроек) =====
    bool _isNight;
    public bool IsNight { get => _isNight; private set { if (_isNight != value) { _isNight = value; Raise(); } } }

    void RecalcNight()
    {
        // локальное время
        var now = DateTime.Now.TimeOfDay;
        var start = _settings.NightStart;  // например 21:00
        var end = _settings.NightEnd;    // например 08:00

        // ночь может «переламывать» сутки (21:00..08:00)
        bool inNight = start <= end ? (now >= start && now < end)
                                    : (now >= start || now < end);

        IsNight = inNight;
    }

    // ===== Мини-игра на улице (чтобы не падали ссылки из GamePage.xaml.cs) =====
    bool _isMiniGame;
    public bool IsMiniGame
    {
        get => _isMiniGame;
        set { if (_isMiniGame != value) { _isMiniGame = value; Raise(); } }
    }

    int _miniLives = 3;
    public int MiniLives { get => _miniLives; set { if (_miniLives != value) { _miniLives = value; Raise(); } } }

    int _miniScore;
    public int MiniScore { get => _miniScore; set { if (_miniScore != value) { _miniScore = value; Raise(); } } }

    // стартовые координаты мини-игры берём/кладём в SettingsService
    public double MiniStartX
    {
        get => _settings.MiniStartX;
        set { if (Math.Abs(_settings.MiniStartX - value) > 0.0001) { _settings.MiniStartX = value; _settings.RaiseSettingsChanged(); } }
    }

    public double MiniTrackY
    {
        get => _settings.MiniTrackY;
        set { if (Math.Abs(_settings.MiniTrackY - value) > 0.0001) { _settings.MiniTrackY = value; _settings.RaiseSettingsChanged(); } }
    }

    // ===== Команды =====
    public ICommand TapMoveCommand { get; }
    public ICommand DropToyCommand { get; }
    public ICommand ToggleLocationCommand { get; }
    public ICommand ToggleMiniGameCommand { get; }

    public GameViewModel(IDatabaseService db, IAudioService audio, ISettingsService settings)
    {
        _db = db;
        _audio = audio;
        _settings = settings;

        _audio.IsEnabled = _settings.SoundEnabled;

        TapMoveCommand = new Command<Point>(async p => await MoveCatAsync(p));
        DropToyCommand = new Command<Toy>(async toy => await CatchToyAsync(toy));
        ToggleLocationCommand = new Command(async () => await ToggleLocationAsync());
        ToggleMiniGameCommand = new Command(() =>
        {
            // мини-игра доступна только на улице и не в ночи
            if (!IsOutside || IsNight) return;
            IsMiniGame = !IsMiniGame;
            // сам запуск/остановку таймеров делает код в GamePage.xaml.cs,
            // он подписан на изменение IsMiniGame.
        });

        // пересчитывать ночь при изменении настроек
        _settings.SettingsChanged += (_, __) => RecalcNight();
    }

    public async Task InitAsync()
    {
        await _db.InitAsync();
        var toys = await _db.GetToysAsync();
        Toys.Clear();
        foreach (var t in toys) Toys.Add(t);

        RecalcNight();

        // стартуем с комнаты
        IsOutside = false;
        _roomX = CatX; _roomY = CatY;

        // отключим любые фоновые звуки при входе
        _audio.StopLoop();

        _session.Restart();
    }

    public async Task SaveStatsAsync()
    {
        _session.Stop();

        // при уходе со страницы глушим фон
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
        // короткий эффект (по желанию)
        await _audio.PlayAsync("meow.mp3");
    }

    async Task CatchToyAsync(Toy toy)
    {
        ToysCaught++;
        Score += toy.Fun;
        await _audio.PlayAsync("purr.mp3");
    }

    public async Task ToggleLocationAsync()
    {
        // запомним позицию текущей локации
        if (IsOutside) { _outX = CatX; _outY = CatY; }
        else { _roomX = CatX; _roomY = CatY; }

        // переключаемся
        IsOutside = !IsOutside;

        // при входе в комнату — выключаем мини-игру
        if (!IsOutside) IsMiniGame = false;

        // восстановим позицию в новой локации (если ещё не ходили — оставим текущую)
        var targetX = IsOutside ? (_outX != 0 ? _outX : CatX) : (_roomX != 0 ? _roomX : CatX);
        var targetY = IsOutside ? (_outY != 0 ? _outY : CatY) : (_roomY != 0 ? _roomY : CatY);

        CatX = targetX; Raise(nameof(CatX));
        CatY = targetY; Raise(nameof(CatY));

        // фон: луп птиц на улице, тишина в комнате
        if (IsOutside && !IsNight)
            await _audio.PlayLoopAsync("birdsongs.mp3", 0.35);
        else
            _audio.StopLoop();
    }
}
