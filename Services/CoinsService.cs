using Microsoft.Maui.Storage;

namespace CatWorld.Services;

public interface ICoinsService
{
    int Coins { get; set; }
    void Add(int amount);
}

public class CoinsService : ICoinsService
{
    const string Key = "coins.total";
    const int DefaultCoins = 0;

    public int Coins
    {
        get => Preferences.Get(Key, DefaultCoins);
        set => Preferences.Set(Key, Math.Max(0, value));
    }

    public void Add(int amount) => Coins = Coins + amount;
}
