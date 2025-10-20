using CatWorld.ViewModels;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Audio;

namespace CatWorld.Views;

public partial class CatchGamePage : ContentPage
{
    private CatchGameViewModel VM => (CatchGameViewModel)BindingContext;

    readonly Random _rnd = new();
    readonly List<Faller> _fallers = new();

    // ✅ таймеры MAUI
    IDispatcherTimer _frame;    // ~60 FPS
    IDispatcherTimer _spawner;  // спавн предметов

    double _fieldW, _fieldH;

    // конфиг
    readonly string[] _edible =
    {
        "items/banan.png","items/pizza.png","items/burger.png",
        "items/candy.png","items/apple.png","items/cake.png"
    };

    readonly string[] _inedible =
    {
        "items/phone.png","items/book.png","items/doll.png",
        "items/gamepad.png","items/folk.png","items/clock.png"
    };

    public CatchGamePage(CatchGameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // ✅ создаём таймеры через Dispatcher
        _frame = Dispatcher.CreateTimer();
        _frame.Interval = TimeSpan.FromMilliseconds(16);
        _frame.Tick += OnFrame;

        _spawner = Dispatcher.CreateTimer();
        _spawner.Interval = TimeSpan.FromMilliseconds(900);
        _spawner.Tick += (_, __) => { if (VM.IsRunning) Spawn(); };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        VM.ResetCommand.Execute(null);
        _frame.Start();
        _spawner.Start();

        SizeChanged += (_, __) =>
        {
            _fieldW = PlayField.Width;
            _fieldH = PlayField.Height;
        };
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _frame.Stop();
        _spawner.Stop();
        ClearAll();
    }

    void ClearAll()
    {
        foreach (var f in _fallers) PlayField.Remove(f.Image);
        _fallers.Clear();
    }

    // ===== управление котом =====
    private void OnPanCat(object? sender, PanUpdatedEventArgs e)
    {
        if (!VM.IsRunning) return;
        if (e.StatusType == GestureStatus.Running)
        {
            var x = Math.Clamp(Cat.TranslationX + e.TotalX, -_fieldW / 2 + 60, _fieldW / 2 - 60);
            Cat.TranslationX = x;
        }
    }

    private async void OnTapField(object? sender, TappedEventArgs e)
    {
        if (!VM.IsRunning) return;
        var p = e.GetPosition(PlayField) ?? new Point(_fieldW / 2, _fieldH - 60);
        var targetX = Math.Clamp(p.X - _fieldW / 2, -_fieldW / 2 + 60, _fieldW / 2 - 60);
        await Cat.TranslateTo(targetX, Cat.TranslationY, 180, Easing.CubicOut);
    }

    // ===== спавн и кадр =====
    void Spawn()
    {
        if (_fieldW <= 0) return;

        bool edible = _rnd.NextDouble() < 0.6; // 60% съедобные
        string icon = edible ? _edible[_rnd.Next(_edible.Length)]
                             : _inedible[_rnd.Next(_inedible.Length)];

        var img = new Image
        {
            Source = icon,
            WidthRequest = 56,
            HeightRequest = 56,
            TranslationX = _rnd.NextDouble() * (_fieldW - 56) - (_fieldW - 56) / 2,
            TranslationY = -_fieldH / 2 - 30
        };

        var speed = 180 + _rnd.NextDouble() * 140; // px/sec
        var faller = new Faller { Image = img, Speed = speed, IsEdible = edible };

        _fallers.Add(faller);
        AbsoluteLayout.SetLayoutBounds(img, new Rect(.5, .5, 56, 56));
        AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.PositionProportional);
        PlayField.Add(img);
    }

    void OnFrame(object? s, EventArgs e)
    {
        if (!VM.IsRunning) return;
        if (_fieldH <= 0) return;

        double dt = 0.016; // ~60Hz
        var catRect = GetRect(Cat, 90, 30); // хитбокс кота

        // обновляем объекты
        for (int i = _fallers.Count - 1; i >= 0; i--)
        {
            var f = _fallers[i];
            f.Image.TranslationY += f.Speed * dt;

            var fRect = GetRect(f.Image, 40, 40);
            if (Intersects(catRect, fRect))
            {
                PlayField.Remove(f.Image);
                _fallers.RemoveAt(i);

                if (f.IsEdible)
                {
                    VM.Score += 1;
                    _ = PlaySoundAsync("yummy.mp3");   // fire-and-forget
                }
                else
                {
                    VM.Lives -= 1;
                    ShakeCat();
                    _ = PlaySoundAsync("oops.mp3");
                    if (VM.Lives <= 0)
                    {
                        VM.OnGameOver();
                        _ = DisplayAlert("Game Over", $"Score: {VM.Score}", "OK");
                        ClearAll();
                        return;
                    }
                }
                continue;
            }

            if (f.Image.TranslationY > _fieldH / 2 + 40)
            {
                PlayField.Remove(f.Image);
                _fallers.RemoveAt(i);
            }
        }

        // динамический спавн
        var ms = Math.Max(450, 900 - VM.Score * 10);
        if (_spawner.Interval.TotalMilliseconds != ms)
            _spawner.Interval = TimeSpan.FromMilliseconds(ms);
    }

    // ===== утилиты =====
    Rect GetRect(View v, double w, double h)
    {
        double cx = _fieldW / 2 + v.TranslationX;
        double cy = _fieldH / 2 + v.TranslationY;
        return new Rect(cx - w / 2, cy - h / 2, w, h);
    }

    bool Intersects(in Rect a, in Rect b) => a.IntersectsWith(b);

    async void ShakeCat()
    {
        await Cat.TranslateTo(Cat.TranslationX - 10, Cat.TranslationY, 40);
        await Cat.TranslateTo(Cat.TranslationX + 20, Cat.TranslationY, 70);
        await Cat.TranslateTo(Cat.TranslationX - 10, Cat.TranslationY, 40);
    }

    // ✅ проигрывание коротких эффектов через Plugin.Maui.Audio
    async Task PlaySoundAsync(string file)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(file);
            using var player = AudioManager.Current.CreatePlayer(stream);
            player.Volume = 1.0;
            player.Play();
        }
        catch { /* звук необязателен */ }
    }

    class Faller
    {
        public Image Image { get; set; } = default!;
        public double Speed { get; set; }
        public bool IsEdible { get; set; }
    }
}
