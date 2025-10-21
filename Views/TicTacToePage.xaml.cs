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

    // Пробрасываем индексы в VM
    void OnCell0(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(0);
    void OnCell1(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(1);
    void OnCell2(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(2);
    void OnCell3(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(3);
    void OnCell4(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(4);
    void OnCell5(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(5);
    void OnCell6(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(6);
    void OnCell7(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(7);
    void OnCell8(object s, TappedEventArgs e) => VM.TapCellCommand.Execute(8);
}
