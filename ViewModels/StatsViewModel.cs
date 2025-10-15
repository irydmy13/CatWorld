using System.ComponentModel;
using CatWorld.Models;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class StatsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly IDatabaseService _db;

    int _toys; public int Toys { get => _toys; set { _toys = value; OnPropertyChanged(nameof(Toys)); } }
    int _score; public int Score { get => _score; set { _score = value; OnPropertyChanged(nameof(Score)); } }
    long _seconds; public long Seconds { get => _seconds; set { _seconds = value; OnPropertyChanged(nameof(Seconds)); } }

    public StatsViewModel(IDatabaseService db) => _db = db;

    public async Task LoadAsync()
    {
        await _db.InitAsync();
        var stat = await _db.GetStatAsync();
        Toys = stat.ToysCaught;
        Score = stat.Score;
        Seconds = stat.TotalPlaySeconds;
    }
}
