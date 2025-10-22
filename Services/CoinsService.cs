namespace CatWorld.Services
{
    public interface ICoinsService
    {
        int Coins { get; }
        void Add(int delta);
    }

    public class CoinsService : ICoinsService
    {
        const string Key = "coins";
        public int Coins => Preferences.Get(Key, 0);
        public void Add(int delta) => Preferences.Set(Key, Math.Max(0, Coins + delta));
    }
}
