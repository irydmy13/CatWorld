using CatWorld.Models;
using CatWorld.ViewModels;

namespace CatWorld.Views;

public partial class GamePage : ContentPage
{
    private GameViewModel VM => (GameViewModel)BindingContext;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await VM.InitAsync();
    }

    protected override async void OnDisappearing()
    {
        await VM.SaveStatsAsync();
        base.OnDisappearing();
    }

    // Тап по игровой области — кот идёт в точку
    private async void OnPlayAreaTapped(object sender, TappedEventArgs e)
    {
        var p = e.GetPosition(PlayArea);
        if (p is null) return;

        // анимация движения (центрируем по размеру кота)
        await Cat.TranslateTo(p.Value.X - Cat.Width / 2, p.Value.Y - Cat.Height / 2, 300, Easing.CubicOut);

        // синхронизируем VM
        VM.CatX = Cat.TranslationX;
        VM.CatY = Cat.TranslationY;

        // триггерим звук/логику
        if (VM.TapMoveCommand?.CanExecute(p.Value) == true)
            VM.TapMoveCommand.Execute(p.Value);
    }

    // Старт drag игрушки — складываем объект в Data.Properties
    private void OnToyDragStarting(object sender, DragStartingEventArgs e)
    {
        if ((sender as Element)?.BindingContext is Toy toy)
            e.Data.Properties["toy"] = toy;
    }

    // Drop игрушки — кот «поймал»
    private async void OnDropToy(object sender, DropEventArgs e)
    {
        if (e.Data.Properties.TryGetValue("toy", out var obj) && obj is Toy toy)
        {
            await Cat.TranslateTo(Cat.TranslationX, Cat.TranslationY - 15, 100);
            await Cat.TranslateTo(Cat.TranslationX, Cat.TranslationY, 100);
            VM.DropToyCommand?.Execute(toy);
        }
    }
}
