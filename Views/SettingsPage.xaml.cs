namespace CatWorld.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}