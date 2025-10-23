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

    // Тап по зоне: в комнате — свободно; на улице — только по X
    private async void OnPlayAreaTapped(object? sender, TappedEventArgs e)
    {
        var p = e.GetPosition(PlayArea);
        if (p is null) return;

        if (VM.IsOutside)
        {
            double targetX = p.Value.X - Cat.Width / 2;
            double w = PlayArea.Width;
            targetX = Math.Clamp(targetX, -w / 2 + 60, w / 2 - 60);
            await Cat.TranslateTo(targetX, Cat.TranslationY, 220, Easing.CubicOut);
        }
        else
        {
            await Cat.TranslateTo(p.Value.X - Cat.Width / 2,
                                  p.Value.Y - Cat.Height / 2, 300, Easing.CubicOut);
        }

        VM.CatX = Cat.TranslationX;
        VM.CatY = Cat.TranslationY;

        if (VM.TapMoveCommand?.CanExecute(p.Value) == true)
            VM.TapMoveCommand.Execute(p.Value);
    }

    // Горизонтальный pan только на улице
    private void OnPanOutside(object? sender, PanUpdatedEventArgs e)
    {
        if (!VM.IsOutside) return;
        var w = PlayArea.Width;
        if (e.StatusType == GestureStatus.Running)
        {
            var x = Math.Clamp(Cat.TranslationX + e.TotalX, -w / 2 + 60, w / 2 - 60);
            Cat.TranslationX = x;
            VM.CatX = x;
        }
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

    private async void OnOpenTicTacToe(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TicTacToePage));
    }
}
