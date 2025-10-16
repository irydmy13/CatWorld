using CatWorld.ViewModels;
using CatWorld.Models;
using Microsoft.Maui.Controls;

namespace CatWorld.Views;

public partial class GamePage : ContentPage
{
    // Должно быть свойство доступа к VM
    private GameViewModel VM => (GameViewModel)BindingContext;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // ✅ НУЖНА ИМЕННО такая сигнатура для TapGestureRecognizer
    private async void OnPlayAreaTapped(object sender, TappedEventArgs e)
    {
        // координаты тапа относительно PlayArea
        var p = e.GetPosition(PlayArea);
        if (p is null) return;

        // анимация перемещения кота
        await Cat.TranslateTo(p.Value.X - Cat.Width / 2, p.Value.Y - Cat.Height / 2, 300, Easing.CubicOut);

        // синхронизируем VM
        VM.CatX = Cat.TranslationX;
        VM.CatY = Cat.TranslationY;

        // команда VM (звук "мяу")
        if (VM.TapMoveCommand?.CanExecute(p.Value) == true)
            VM.TapMoveCommand.Execute(p.Value);
    }

    // drag старт — кладём игрушку в Data.Properties
    private void OnToyDragStarting(object sender, DragStartingEventArgs e)
    {
        if ((sender as Element)?.BindingContext is Toy toy)
            e.Data.Properties["toy"] = toy;
    }

    // drop на игровую зону — кот "поймал" игрушку
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
