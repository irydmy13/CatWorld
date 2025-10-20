using CatWorld.Models;
using CatWorld.ViewModels;
using Microsoft.Maui.Layouts;

namespace CatWorld.Views;

public partial class GamePage : ContentPage
{
    private GameViewModel VM => (GameViewModel)BindingContext;

    // ===== Мини-игра «падения на улице» =====
    IDispatcherTimer? _mgFrame;
    IDispatcherTimer? _mgSpawn;
    readonly Random _rnd = new();
    readonly List<Faller> _items = new();
    double _w, _h;

    readonly string[] MG_EDIBLE =
    {
        "items/banan.png","items/pizza.png","items/burger.png",
        "items/candy.png","items/apple.png","items/cake.png"
    };
    readonly string[] MG_TRASH =
    {
        "items/phone.png","items/book.png","items/doll.png",
        "items/gamepad.png","items/folk.png","items/clock.png"
    };

    // ===== мордочки кота =====
    readonly string CAT_NORMAL = "cat.png";
    readonly string CAT_EAT = "cat_um.png";
    bool _eatFaceOn;
    CancellationTokenSource? _eatCts;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        VM.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(VM.IsOutside))
            {
                if (!VM.IsOutside) { VM.IsMiniGame = false; StopMini(); }
                MiniBtn.IsEnabled = VM.IsOutside;
            }
            else if (e.PropertyName == nameof(VM.IsMiniGame))
            {
                MiniBtn.Text = VM.IsMiniGame ? "Stop" : "Play";
                if (VM.IsOutside && VM.IsMiniGame) StartMini(); else StopMini();
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await VM.InitAsync();

        SizeChanged += (_, __) => { _w = PlayArea.Width; _h = PlayArea.Height; };
        MiniBtn.IsEnabled = VM.IsOutside;
        MiniBtn.Text = VM.IsMiniGame ? "Stop" : "Play";
        ResetCatFace();
    }

    protected override async void OnDisappearing()
    {
        await VM.SaveStatsAsync();
        StopMini();
        base.OnDisappearing();
    }

    // Тап по игровой области — кот идёт в точку
    private async void OnPlayAreaTapped(object sender, TappedEventArgs e)
    {
        var p = e.GetPosition(PlayArea);
        if (p is null) return;

        await Cat.TranslateTo(p.Value.X - Cat.Width / 2, p.Value.Y - Cat.Height / 2, 300, Easing.CubicOut);

        VM.CatX = Cat.TranslationX;
        VM.CatY = Cat.TranslationY;

        if (VM.TapMoveCommand?.CanExecute(p.Value) == true)
            VM.TapMoveCommand.Execute(p.Value);
    }

    private void OnToyDragStarting(object sender, DragStartingEventArgs e)
    {
        if ((sender as Element)?.BindingContext is Toy toy)
            e.Data.Properties["toy"] = toy;
    }

    private async void OnDropToy(object sender, DropEventArgs e)
    {
        if (e.Data.Properties.TryGetValue("toy", out var obj) && obj is Toy toy)
        {
            await Cat.TranslateTo(Cat.TranslationX, Cat.TranslationY - 15, 100);
            await Cat.TranslateTo(Cat.TranslationX, Cat.TranslationY, 100);
            VM.DropToyCommand?.Execute(toy);
        }
    }

    // ===== мини-игра =====
    void EnsureMiniTimers()
    {
        if (_mgFrame != null) return;

        _mgFrame = Dispatcher.CreateTimer();
        _mgFrame.Interval = TimeSpan.FromMilliseconds(16);
        _mgFrame.Tick += MiniFrame;

        _mgSpawn = Dispatcher.CreateTimer();
        _mgSpawn.Interval = TimeSpan.FromMilliseconds(900);
        _mgSpawn.Tick += (_, __) => { if (VM.IsOutside && VM.IsMiniGame) MiniSpawn(); };
    }

    void StartMini()
    {
        if (!VM.IsOutside) return;

        EnsureMiniTimers();
        FallLayer.IsVisible = true;
        VM.MiniScore = 0; VM.MiniLives = 3;
        _mgFrame!.Start();
        _mgSpawn!.Start();
        ResetCatFace();
    }

    void StopMini()
    {
        _mgFrame?.Stop();
        _mgSpawn?.Stop();
        FallLayer.IsVisible = false;
        foreach (var it in _items) FallLayer.Remove(it.Image);
        _items.Clear();
        ResetCatFace();
    }

    void MiniSpawn()
    {
        if (_w <= 0) return;

        bool edible = _rnd.NextDouble() < 0.6;
        string icon = edible ? MG_EDIBLE[_rnd.Next(MG_EDIBLE.Length)]
                             : MG_TRASH[_rnd.Next(MG_TRASH.Length)];

        var img = new Image
        {
            Source = icon,
            WidthRequest = 56,
            HeightRequest = 56,
            TranslationX = _rnd.NextDouble() * (_w - 56) - (_w - 56) / 2,
            TranslationY = -_h / 2 - 30
        };

        var speed = 180 + _rnd.NextDouble() * 140;
        var f = new Faller { Image = img, Speed = speed, IsEdible = edible };

        _items.Add(f);
        AbsoluteLayout.SetLayoutBounds(img, new Rect(.5, .5, 56, 56));
        AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.PositionProportional);
        FallLayer.Add(img);
    }

    void MiniFrame(object? s, EventArgs e)
    {
        if (!(VM.IsOutside && VM.IsMiniGame)) { StopMini(); return; }
        if (_h <= 0) return;

        double dt = 0.016;
        var catRect = GetRect(Cat, 90, 30);

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            var f = _items[i];
            f.Image.TranslationY += f.Speed * dt;

            // --- если съедобное рядом — показать «едальную» мордочку на мгновение
            if (f.IsEdible)
            {
                double cx = _w / 2 + Cat.TranslationX;
                double cy = _h / 2 + Cat.TranslationY;
                double fx = _w / 2 + f.Image.TranslationX;
                double fy = _h / 2 + f.Image.TranslationY;
                double dx = cx - fx, dy = cy - fy;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist < 120 && !_eatFaceOn) _ = ShowEatFaceAsync(200);
            }

            var r = GetRect(f.Image, 40, 40);
            if (catRect.IntersectsWith(r))
            {
                FallLayer.Remove(f.Image);
                _items.RemoveAt(i);

                if (f.IsEdible)
                {
                    VM.MiniScore += 1;
                    _ = ShowEatFaceAsync(500); // подольше при поимке еды
                }
                else
                {
                    VM.MiniLives -= 1;
                    ResetCatFace();            // на несъедобное — обычная мордочка
                    _ = ShakeCat();
                    if (VM.MiniLives <= 0)
                    {
                        VM.IsMiniGame = false;
                        StopMini();
                        _ = DisplayAlert("Game Over", $"Score: {VM.MiniScore}", "OK");
                        return;
                    }
                }
                continue;
            }

            if (f.Image.TranslationY > _h / 2 + 40)
            {
                FallLayer.Remove(f.Image);
                _items.RemoveAt(i);
            }
        }

        var ms = Math.Max(450, 900 - VM.MiniScore * 10);
        if (_mgSpawn!.Interval.TotalMilliseconds != ms)
            _mgSpawn.Interval = TimeSpan.FromMilliseconds(ms);
    }

    Rect GetRect(View v, double w, double h)
    {
        double cx = _w / 2 + v.TranslationX;
        double cy = _h / 2 + v.TranslationY;
        return new Rect(cx - w / 2, cy - h / 2, w, h);
    }

    async Task ShakeCat()
    {
        await Cat.TranslateTo(Cat.TranslationX - 10, Cat.TranslationY, 40);
        await Cat.TranslateTo(Cat.TranslationX + 20, Cat.TranslationY, 70);
        await Cat.TranslateTo(Cat.TranslationX - 10, Cat.TranslationY, 40);
    }

    // ===== мордочки: утилиты =====
    void ResetCatFace()
    {
        try { _eatCts?.Cancel(); } catch { }
        _eatCts = null;
        _eatFaceOn = false;
        Cat.Source = CAT_NORMAL;
    }

    async Task ShowEatFaceAsync(int durationMs = 300)
    {
        if (_eatFaceOn) return;
        _eatFaceOn = true;
        Cat.Source = CAT_EAT;

        var cts = new CancellationTokenSource();
        _eatCts = cts;
        try { await Task.Delay(durationMs, cts.Token); }
        catch (TaskCanceledException) { /* отмена */ }

        if (!cts.IsCancellationRequested)
        {
            Cat.Source = CAT_NORMAL;
            _eatFaceOn = false;
        }
    }

    // ===== внутренняя модель падающего предмета =====
    class Faller
    {
        public Image Image { get; set; } = default!;
        public double Speed { get; set; }
        public bool IsEdible { get; set; }
    }
}
