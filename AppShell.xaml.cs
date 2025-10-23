using CatWorld.Views;

namespace CatWorld;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // чтобы Shell.GoToAsync(nameof(TicTacToePage)) работал:
        Routing.RegisterRoute(nameof(TicTacToePage), typeof(TicTacToePage));
    }
}
