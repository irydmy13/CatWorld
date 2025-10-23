namespace CatWorld.Services;

public interface ICoinsService
{
    int Coins { get; }
    void Add(int amount);
}
