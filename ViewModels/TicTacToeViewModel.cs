using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CatWorld.Services;

namespace CatWorld.ViewModels;

public class TicTacToeViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    // ===== Модель клетки =====
    public class Cell : INotifyPropertyChanged
    {
        string _owner = "";
        string _image = "";
        public string Owner { get => _owner; set { _owner = value; PropertyChanged?.Invoke(this, new(nameof(Owner))); } }
        public string Image { get => _image; set { _image = value; PropertyChanged?.Invoke(this, new(nameof(Image))); } }
        public bool IsEmpty => Owner == "";
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    readonly ICoinsService _coins;

    public ObservableCollection<Cell> Board { get; } =
        new(Enumerable.Range(0, 9).Select(_ => new Cell()));

    // Стикеры
    public string BotImage { get; } = "paw.png"; // кот всегда лапкой

    string _playerImage = "candy.png";           // дефолт
    public string PlayerImage
    {
        get => _playerImage;
        set { if (_playerImage != value) { _playerImage = value; OnPropertyChanged(); } }
    }

    // Всплывающее окно выбора
    bool _isPickerVisible;
    public bool IsPickerVisible { get => _isPickerVisible; set { _isPickerVisible = value; OnPropertyChanged(); } }

    // Пока не выбрали стикер — играть нельзя
    bool _awaitSticker;

    // Состояние
    bool _isGameOver;
    public bool IsGameOver { get => _isGameOver; set { _isGameOver = value; OnPropertyChanged(); } }

    bool _isPlayerTurn = true;
    public bool IsPlayerTurn { get => _isPlayerTurn; set { _isPlayerTurn = value; OnPropertyChanged(); } }

    string _status = "New game — choose a sticker";
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    int _coinsBalance;
    public int CoinsBalance { get => _coinsBalance; set { _coinsBalance = value; OnPropertyChanged(); } }

    bool _botThinking;

    // Пауза «раздумья» кота
    const int BotDelayMs = 2000;

    // Команды
    public ICommand NewGameCommand { get; }
    public ICommand TapCellCommand { get; }
    public ICommand PickStickerCommand { get; }   // выбор стикера и старт

    public TicTacToeViewModel(ICoinsService coins)
    {
        _coins = coins;
        CoinsBalance = _coins.Coins;

        NewGameCommand = new Command(NewGame);
        TapCellCommand = new Command<int>(TapCell);
        PickStickerCommand = new Command<string>(PickSticker);
    }

    // ===== Логика =====
    void NewGame()
    {
        foreach (var c in Board) { c.Owner = ""; c.Image = ""; }
        IsGameOver = false;
        IsPlayerTurn = true;
        _botThinking = false;

        // Блокируем поле до выбора стикера
        _awaitSticker = true;
        IsPickerVisible = true;
        Status = "Choose a sticker for the game";
    }

    void PickSticker(string? src)
    {
        if (!string.IsNullOrWhiteSpace(src))
            PlayerImage = src;

        // Разблокируем поле
        _awaitSticker = false;
        IsPickerVisible = false;
        Status = "Your move";
    }

    void TapCell(int idx)
    {
        if (_awaitSticker) return;          // ещё не выбрали стикер
        if (IsGameOver || !IsPlayerTurn || _botThinking) return;
        if (idx < 0 || idx > 8) return;

        var cell = Board[idx];
        if (!cell.IsEmpty) return;

        Place("P", idx, PlayerImage);

        if (FinishIfNeeded()) return;

        IsPlayerTurn = false;
        _ = BotMoveAsync();
    }

    async Task BotMoveAsync()
    {
        _botThinking = true;
        Status = "The cat is thinking…";

        await Task.Delay(BotDelayMs);

        var cur = Snapshot();
        int bestIdx = -1, bestScore = int.MinValue;

        for (int i = 0; i < 9; i++)
        {
            if (cur[i] != "") continue;
            cur[i] = "B";
            int score = Minimax(cur, isMaximizing: false, depth: 0);
            cur[i] = "";
            if (score > bestScore) { bestScore = score; bestIdx = i; }
        }
        if (bestIdx == -1) bestIdx = Enumerable.Range(0, 9).First(i => Board[i].IsEmpty);

        Place("B", bestIdx, BotImage);

        _botThinking = false;

        if (FinishIfNeeded()) return;

        IsPlayerTurn = true;
        Status = "Your turn";
    }

    void Place(string who, int idx, string imageSrc)
    {
        Board[idx].Owner = who;
        Board[idx].Image = imageSrc;
    }

    bool FinishIfNeeded()
    {
        var win = Winner();
        if (win != "")
        {
            IsGameOver = true;
            bool playerWon = (win == "P");
            Status = playerWon ? "Victory! +5" : "The cat won! +1";
            _coins.Add(playerWon ? 5 : 1);
            CoinsBalance = _coins.Coins;
            return true;
        }
        if (Board.All(c => !c.IsEmpty))
        {
            IsGameOver = true;
            Status = "Draw +2";
            _coins.Add(2);
            CoinsBalance = _coins.Coins;
            return true;
        }
        return false;
    }

    string Winner()
    {
        int[][] lines = new[]
        {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
            new[]{0,4,8}, new[]{2,4,6}
        };

        foreach (var l in lines)
        {
            var a = Board[l[0]].Owner; if (a == "") continue;
            if (a == Board[l[1]].Owner && a == Board[l[2]].Owner) return a;
        }
        return "";
    }

    // Minimax
    int Minimax(string[] cur, bool isMaximizing, int depth)
    {
        string w = WinnerOf(cur);
        if (w == "B") return 10 - depth;
        if (w == "P") return depth - 10;
        if (cur.All(c => c != "")) return 0;

        int best = isMaximizing ? int.MinValue : int.MaxValue;

        for (int i = 0; i < 9; i++)
        {
            if (cur[i] != "") continue;
            cur[i] = isMaximizing ? "B" : "P";
            int score = Minimax(cur, !isMaximizing, depth + 1);
            cur[i] = "";
            best = isMaximizing ? Math.Max(best, score) : Math.Min(best, score);
        }
        return best;
    }

    string WinnerOf(string[] cur)
    {
        int[][] lines = new[]
        {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
            new[]{0,4,8}, new[]{2,4,6}
        };
        foreach (var l in lines)
        {
            var a = cur[l[0]]; if (a == "") continue;
            if (a == cur[l[1]] && a == cur[l[2]]) return a;
        }
        return "";
    }

    string[] Snapshot() => Board.Select(c => c.Owner).ToArray();
}
