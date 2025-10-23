using Microsoft.Maui.Storage;

namespace CatWorld.Services;

public class CoinsService : ICoinsService
{
    const string Key = "coins";
    public int Coins => Preferences.Get(Key, 0);

    public void Add(int amount)
    {
        var v = Math.Max(0, Coins + amount);
        Preferences.Set(Key, v);
    }
}
