using CatWorld.ViewModels;

namespace CatWorld.Views;

public partial class TicTacToePage : ContentPage
{
    TicTacToeViewModel VM => (TicTacToeViewModel)BindingContext;

    public TicTacToePage(TicTacToeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    void Exec(int i)
    {
        if (VM?.TapCellCommand?.CanExecute(i) == true)
            VM.TapCellCommand.Execute(i);
    }

    void OnCell0(object s, TappedEventArgs e) => Exec(0);
    void OnCell1(object s, TappedEventArgs e) => Exec(1);
    void OnCell2(object s, TappedEventArgs e) => Exec(2);
    void OnCell3(object s, TappedEventArgs e) => Exec(3);
    void OnCell4(object s, TappedEventArgs e) => Exec(4);
    void OnCell5(object s, TappedEventArgs e) => Exec(5);
    void OnCell6(object s, TappedEventArgs e) => Exec(6);
    void OnCell7(object s, TappedEventArgs e) => Exec(7);
    void OnCell8(object s, TappedEventArgs e) => Exec(8);
}
