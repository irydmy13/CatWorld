using System.ComponentModel;
using System.Windows.Input;

namespace CatWorld.ViewModels;

public class CatchGameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    int _score;
    public int Score { get => _score; set { if (_score != value) { _score = value; Raise(nameof(Score)); } } }

    int _lives = 3;
    public int Lives { get => _lives; set { if (_lives != value) { _lives = value; Raise(nameof(Lives)); } } }

    int _best;
    public int Best { get => _best; set { if (_best != value) { _best = value; Raise(nameof(Best)); } } }

    bool _running;
    public bool IsRunning { get => _running; set { if (_running != value) { _running = value; Raise(nameof(IsRunning)); } } }

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResetCommand { get; }

    public CatchGameViewModel()
    {
        Best = Preferences.Get("catch_best", 0);
        StartCommand = new Command(() => IsRunning = true);
        PauseCommand = new Command(() => IsRunning = false);
        ResetCommand = new Command(() => { Score = 0; Lives = 3; });
    }

    public void OnGameOver()
    {
        IsRunning = false;
        if (Score > Best)
        {
            Best = Score;
            Preferences.Set("catch_best", Best);
        }
    }
}
