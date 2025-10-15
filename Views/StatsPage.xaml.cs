using CatWorld.ViewModels;

namespace CatWorld.Views;

public partial class StatsPage : ContentPage
{
    StatsViewModel VM => (StatsViewModel)BindingContext;
    public StatsPage(StatsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await VM.LoadAsync();
    }
}
