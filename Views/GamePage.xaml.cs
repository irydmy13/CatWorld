using CatWorld.ViewModels;

namespace CatWorld.Views;

public partial class GamePage : ContentPage
{
	public GamePage(GameViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}